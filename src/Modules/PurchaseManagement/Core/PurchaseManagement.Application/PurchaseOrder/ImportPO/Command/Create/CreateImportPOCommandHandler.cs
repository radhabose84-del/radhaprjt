using System.Text.Json;
using AutoMapper;
using Contracts.Commands.Workflow;
using Contracts.Events.Notifications;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Budget;
using Contracts.Interfaces.Lookups.Common;
using Contracts.Interfaces.Lookups.Finance;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.Common.Interfaces.IOutbox;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ImportPO;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IPurchaseDocument;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.PurchaseOrder;
using PurchaseManagement.Domain.PurchaseOrder;

namespace PurchaseManagement.Application.PurchaseOrder.ImportPO.Command.Create;

public class CreateImportPOCommandHandler : IRequestHandler<CreateImportPOCommand, int>
{
    private readonly IImportPOCommandRepository _repo;
    private readonly IIPAddressService _ip;
    private readonly ITimeZoneService _tz;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateImportPOCommandHandler> _logger;
    private readonly IMiscMasterQueryRepository _misc;
    private readonly IPODocumentQueryRepository _poDocumentQueryRepository;
    private readonly IImportPOQueryRepository _purchaseOrderQueryRepository;
    private readonly IBudgetAllocationLookup _budgetAllocationLookup;
    private readonly IDocumentSequenceLookup _documentSequenceLookup;
    private readonly IOutboxEventPublisher _outboxEventPublisher;
    private readonly IAppDataMiscMasterLookup _appDataMiscLookup;

    public CreateImportPOCommandHandler(
        IImportPOCommandRepository repo,
        IMapper mapper,
        IIPAddressService ip,
        ITimeZoneService tz,
        ILogger<CreateImportPOCommandHandler> logger,
        IMiscMasterQueryRepository misc,
        IPODocumentQueryRepository poDocumentQueryRepository,
        IImportPOQueryRepository purchaseOrderQueryRepository,
        IBudgetAllocationLookup budgetAllocationLookup,
        IDocumentSequenceLookup documentSequenceLookup,
        IOutboxEventPublisher outboxEventPublisher,
        IAppDataMiscMasterLookup appDataMiscLookup)
    {
        _repo = repo; _mapper = mapper; _ip = ip; _tz = tz; _logger = logger;
        _misc = misc; _poDocumentQueryRepository = poDocumentQueryRepository;
        _purchaseOrderQueryRepository = purchaseOrderQueryRepository;
        _budgetAllocationLookup = budgetAllocationLookup ?? throw new ArgumentNullException(nameof(budgetAllocationLookup));
        _documentSequenceLookup = documentSequenceLookup;
        _outboxEventPublisher = outboxEventPublisher;
        _appDataMiscLookup = appDataMiscLookup;
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

        // Determine transaction type based on PO category (Emergency overrides default)
        var poCategory = await _misc.GetByIdAsync(dto.POCategoryId);
        var isEmergency = string.Equals(poCategory?.Description, MiscEnumEntity.EmergencyPO, StringComparison.OrdinalIgnoreCase);
        var transactionTypeName = isEmergency
            ? MiscEnumEntity.TransactionTypeEPO
            : MiscEnumEntity.TransactionTypeIPO;

        var transactionTypeId = await _documentSequenceLookup.GetTransactionTypeIdAsync(
            transactionTypeName, MiscEnumEntity.ModulePurchase, entity.UnitId)
            ?? throw new InvalidOperationException("No transaction type configured for PO.");
        var sequences = await _documentSequenceLookup.GenerateDocumentNumber(transactionTypeId);
        entity.PONumber = sequences.Count > 0
            ? sequences[^1]
            : throw new InvalidOperationException("No document sequence configured for PO.");
        entity.CreatedBy = _ip.GetUserId();
        entity.CreatedByName = _ip.GetUserName();
        entity.CreatedIP = _ip.GetSystemIPAddress();
        entity.CreatedDate = now;

        var pending = await _misc.GetMiscMasterByName(MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Pending);
        entity.StatusId = pending.Id;

        // Documents: filter + rename on disk, then attach to entity.PurchaseDocuments
        if (dto.Documents != null && dto.Documents.Any())
        {
            dto.Documents = dto.Documents
                .Where(d => d.DocumentId != 0 && !string.Equals(d.FileName, "string", StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (dto.Documents.Any())
            {
                var baseDirectory = MiscEnumEntity.DocumentPath;
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

                // Attach to aggregate so EF saves with the graph
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

            // ── Approval / Notification (outbox — same transaction) ──────────────
            var correlationId = Guid.NewGuid();
            var createdByName = entity.CreatedByName ?? string.Empty;

            var workFlowEntity = await _repo.GetByIdImportPOWorkFlowAsync(poId);
            var reversePayload = new CreateImportPOReverseDto
            {
                Header = workFlowEntity,
                Lines = null
            };
            var serializedPayload = JsonSerializer.Serialize(reversePayload);

            var workflowCommand = new CreateApprovalRequestCommand
            {
                CorrelationId = correlationId,
                ModuleTypeName = MiscEnumEntity.POLocal,
                ModuleTransactionId = poId,
                Payload = serializedPayload,
                TransactionTypeId = transactionTypeId
            };

          /*   var notificationEventMisc = await _appDataMiscLookup.GetMiscMasterByNameAsync(
                NotificationEnum.NotificationEvent, NotificationEnum.Create);

            var notificationEvent = new NotificationCreatedEvent
            {
                CorrelationId = correlationId,
                CreatedByName = createdByName,
                UnitId = entity.UnitId,
                ModuleName = "Purchase Order",
                EventTypeId = notificationEventMisc?.Id ?? 0,
                param1 = entity.PONumber,
                param2 = createdByName,
                param3 = entity.PODate,
                param4 = entity.PurchaseValue.ToString(),
                param5 = dto.VendorId.ToString(),
                ModuleTransactionId = poId,
                ModuleTypeName = MiscEnumEntity.POImport
            }; */

            await _outboxEventPublisher.ScheduleWithoutSaveAsync(workflowCommand, correlationId, ct);
          //  await _outboxEventPublisher.ScheduleWithoutSaveAsync(notificationEvent, correlationId, ct);

            await _repo.SaveChangesAsync(ct);

            // Increment DocNo via lookup — same connection + transaction
            await _documentSequenceLookup.IncrementDocNoAsync(transactionTypeId, dbConn, dbTx);

            await transaction.CommitAsync(ct);

            _logger.LogInformation(
                "Import PO {PoNumber} created with CorrelationId {CorrelationId}. Transaction committed. Events scheduled to outbox.",
                entity.PONumber, correlationId);

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
