using AutoMapper;
using Contracts.Interfaces.Lookups.Budget;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ImportPO;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IPurchaseDocument;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.Local;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.PurchaseOrder;
using PurchaseManagement.Domain.PurchaseOrder;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace PurchaseManagement.Application.PurchaseOrder.ImportPO.Command.Create;

public class CreateImportPOCommandHandler : IRequestHandler<CreateImportPOCommand, int>
{
    private readonly IImportPOCommandRepository _repo;
    private readonly IIPAddressService _ip;
    private readonly ITimeZoneService _tz;
    private readonly IMapper _mapper;
    private readonly IPurchaseOrderCommandRepository _poRepo;
    private readonly ILogger<CreateImportPOCommandHandler> _logger;
    private readonly IMiscMasterQueryRepository _misc;
    private readonly IPODocumentQueryRepository _poDocumentQueryRepository;
    private readonly IImportPOQueryRepository _purchaseOrderQueryRepository;    
    private readonly IUnitLookup _unitLookup;
    private readonly IBudgetAllocationLookup _budgetAllocationLookup;    

    public CreateImportPOCommandHandler(
        IImportPOCommandRepository repo,
        IMapper mapper,
        IIPAddressService ip,
        ITimeZoneService tz,
        ILogger<CreateImportPOCommandHandler> logger,
        IMiscMasterQueryRepository misc,
        IPurchaseOrderCommandRepository poRepo,
        IPODocumentQueryRepository poDocumentQueryRepository,  IImportPOQueryRepository purchaseOrderQueryRepository, IUnitLookup unitLookup, IBudgetAllocationLookup budgetAllocationLookup
        )
    {
        _repo = repo; _mapper = mapper; _ip = ip; _tz = tz; _logger = logger;
        _misc = misc; _poRepo = poRepo; _poDocumentQueryRepository = poDocumentQueryRepository;_purchaseOrderQueryRepository = purchaseOrderQueryRepository; 
        _unitLookup = unitLookup ?? throw new ArgumentNullException(nameof(unitLookup));
        _budgetAllocationLookup = budgetAllocationLookup ?? throw new ArgumentNullException(nameof(budgetAllocationLookup));
    }

    public async Task<int> Handle(CreateImportPOCommand request, CancellationToken ct)
    {
        var dto = request.Data;
        var entity = _mapper.Map<PurchaseOrderHeader>(dto);

        // audit
        var tzId = _tz.GetSystemTimeZone();
        TimeZoneInfo tzi;
        try { tzi = TimeZoneInfo.FindSystemTimeZoneById(tzId); }
        catch { tzi = TimeZoneInfo.Local; }
        var now = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, tzi);

        entity.UnitId = _ip.GetUnitId() ?? 0;

        var units = await _unitLookup.GetAllUnitAsync();
        var unitLookupDict = units.ToDictionary(u => u.UnitId, u => (u.ShortName ?? string.Empty).Trim());

        if (!unitLookupDict.TryGetValue(entity.UnitId, out var unitCode) || string.IsNullOrWhiteSpace(unitCode))
        {
            var msg = $"Invalid UnitId {entity.UnitId}. Failed to generate PO number.";
            _logger.LogWarning(msg);
            throw new InvalidOperationException(msg);
        }

        entity.PONumber = await _poRepo.GenerateNextCodeAsync(
                entity.POCategoryId,
                entity.POMethodId,
                entity.PODate,
                unitCode,
                ct);
        entity.CreatedBy = _ip.GetUserId();
        entity.CreatedByName = _ip.GetUserName();
        entity.CreatedIP = _ip.GetSystemIPAddress();
        entity.CreatedDate = now;

        // 🔐 Documents: filter + rename on disk, then attach to entity.PurchaseDocuments
        if (dto.Documents != null && dto.Documents.Any())
        {
            dto.Documents = dto.Documents
                .Where(d => d.DocumentId != 0 && !string.Equals(d.FileName, "string", StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (dto.Documents.Any())
            {
                var baseDirectory = MiscEnumEntity.DocumentPath; // or await _poDocumentQueryRepository.GetBaseDirectoryAsync();
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", baseDirectory);
                EnsureDirectoryExists(uploadPath);

                foreach (var doc in dto.Documents)
                {
                    if (string.IsNullOrWhiteSpace(doc.FileName)) continue;

                    var oldFilePath = Path.Combine(uploadPath, doc.FileName);
                    if (File.Exists(oldFilePath))
                    {
                        var newFileName = $"{entity.PONumber}_{doc.DocumentId}{Path.GetExtension(oldFilePath)}";
                        var newFilePath = Path.Combine(uploadPath, newFileName);
                        File.Move(oldFilePath, newFilePath, overwrite: true);
                        doc.FileName = newFileName;
                        if (doc.UploadedDate == default) doc.UploadedDate = DateTimeOffset.UtcNow;
                    }
                }
                
                // ✅ Attach to aggregate so EF saves with the graph
                entity.PurchaseDocumentTypes = dto.Documents.Select(d => new PurchaseDocument
                {
                    DocumentId = d.DocumentId,
                    FileName = d.FileName,
                    UploadedDate = d.UploadedDate
                }).ToList();
            }
        }

        // ── Shared Transaction Pattern ────────────────────────────────────────────
        // Both the Import PO EF Core write and the Budget Dapper UPDATE must commit
        // or roll back together. We open the transaction here (handler level), pass
        // the underlying ADO.NET connection + transaction to the Budget lookup so it
        // joins the same SQL transaction, then commit once both succeed.
        // ─────────────────────────────────────────────────────────────────────────
        var strategy = _repo.CreateExecutionStrategy();
        var result = await strategy.ExecuteAsync(async () =>
        {
            var (transaction, dbConn, dbTx) = await _repo.BeginTransactionWithConnectionAsync(ct);
            await using var _ = transaction;

            var poId = await _repo.CreateWithoutTransactionAsync(entity, dto, ct);

            if (poId > 0 && entity.BudgetGroupId.HasValue && entity.BudgetGroupId.Value > 0 && entity.PurchaseValue > 0)
            {
                var budgetMonthDate = new DateOnly(entity.PODate.Year, entity.PODate.Month, 1);

                // Negative delta consumes budget. The lookup SQL is
                // `RemainingBalance = RemainingBalance + @DeltaAmount`, so a PO
                // (which spends budget) must pass a negative delta.
                var ok = await _budgetAllocationLookup.ApplyRemainingBalanceDeltaAsync(
                    entity.BudgetGroupId.Value,
                    budgetMonthDate,
                    entity.BudgetMonthId ?? 0,
                    entity.BudgetRequestById ?? 0,
                    -entity.PurchaseValue,
                    entity.ProjectId,
                    entity.WBSId,
                    entity.FinancialYearId,
                    dbConn,
                    dbTx,
                    ct);

                if (!ok)
                {
                    _logger.LogWarning(
                        "Import PO: Budget consume failed. PO {PoId}, BG {BgId}, Delta {Delta}",
                        poId,
                        entity.BudgetGroupId,
                        entity.PurchaseValue);
                }
            }

            await transaction.CommitAsync(ct);
            return poId;
        });

        return result;
    }

    private static void EnsureDirectoryExists(string path)
    {
        if (!string.IsNullOrWhiteSpace(path) && !Directory.Exists(path))
            Directory.CreateDirectory(path);
    }
}
