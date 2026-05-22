#nullable disable
using System.Text.Json;
using AutoMapper;
using Contracts.Commands.Workflow;
using Contracts.Events.Notifications;
using Contracts.Interfaces.Lookups.Budget;
using Contracts.Interfaces.Lookups.Common;
using Contracts.Interfaces.Lookups.Finance;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Common;
using Contracts.Interfaces;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.Common.Interfaces.IOutbox;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IPurchaseDocument;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.Local;
using LocalDtos = PurchaseManagement.Application.PurchaseOrder.Dtos.Local;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.PurchaseOrder;
using PurchaseManagement.Domain.Entities.PurchaseOrder.Local;
using PurchaseManagement.Domain.PurchaseOrder;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Microsoft.Extensions.Logging;

namespace PurchaseManagement.Application.PurchaseOrder.Local.Commands.Create
{
    public class CreatePurchaseOrderCommandHandler
        : IRequestHandler<CreatePurchaseOrderCommand, ApiResponseDTO<int>>
    {
        private readonly IPurchaseOrderCommandRepository _repo;
        private readonly IMapper _mapper;
        private readonly IIPAddressService _ip;
        private readonly ITimeZoneService _timeZoneService;
        private readonly ILogger<CreatePurchaseOrderCommandHandler> _logger;
        private readonly IOutboxEventPublisher _outboxEventPublisher;
        private readonly IPODocumentQueryRepository _poDocumentQueryRepository;
        private readonly IUnitLookup _unitLookup;
        private readonly ICompanyLookup _companyLookup;
        private readonly IBudgetAllocationLookup _budgetAllocationLookup;
        private readonly IFinancialYearLookup _financialYearLookup;
        private readonly IAppDataMiscMasterLookup _appDataMiscLookup;
        private readonly IDocumentSequenceLookup _documentSequenceLookup;
        private readonly IMiscMasterQueryRepository _misc;

        public CreatePurchaseOrderCommandHandler(
            IPurchaseOrderCommandRepository repo,
            IMapper mapper,
            IIPAddressService ip,
            ITimeZoneService timeZoneService,
            ILogger<CreatePurchaseOrderCommandHandler> logger,
            IOutboxEventPublisher outboxEventPublisher,
            IPODocumentQueryRepository poDocumentQueryRepository,
            IUnitLookup unitLookup,
            ICompanyLookup companyLookup,
            IBudgetAllocationLookup budgetAllocationLookup,
            IFinancialYearLookup financialYearLookup,
            IAppDataMiscMasterLookup appDataMiscLookup,
            IDocumentSequenceLookup documentSequenceLookup,
            IMiscMasterQueryRepository misc)
        {
            _repo = repo;
            _mapper = mapper;
            _ip = ip ?? throw new ArgumentNullException(nameof(ip));
            _timeZoneService = timeZoneService;
            _logger = logger;
            _outboxEventPublisher = outboxEventPublisher;
            _poDocumentQueryRepository = poDocumentQueryRepository;
            _unitLookup = unitLookup;
            _companyLookup = companyLookup;
            _budgetAllocationLookup = budgetAllocationLookup ??
                                      throw new ArgumentNullException(nameof(budgetAllocationLookup));
            _financialYearLookup = financialYearLookup;
            _appDataMiscLookup = appDataMiscLookup;
            _documentSequenceLookup = documentSequenceLookup;
            _misc = misc;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreatePurchaseOrderCommand request, CancellationToken ct)
        {
            var dto = request.Data;
            if (dto == null)
            {
                return new ApiResponseDTO<int>
                {
                    IsSuccess = false,
                    Message = "Invalid request payload.",
                    Data = 0
                };
            }

            var requestDate = DateOnly.FromDateTime(request.Data.PODate.DateTime);

          /*   if (request.Data.FinancialYearId <= 0 || request.Data.FinancialYearId == null)
            {
                var financialYears = await _financialYearLookup
                    .GetAllFinancialYearAsync();

                // convert DateTimeOffset → DateOnly for comparison
                var fyMatch = financialYears
                    .FirstOrDefault(fy =>
                    {
                        var start = DateOnly.FromDateTime(fy.StartDate.Date);
                        var end = DateOnly.FromDateTime(fy.EndDate.Date);
                        return start <= requestDate && requestDate <= end;
                    });

                if (fyMatch != null)
                {
                    request.Data.FinancialYearId = fyMatch.FinancialYearId;
                }
                else
                {
                    var msg = $"Financial year does not exist for date {requestDate:yyyy-MM-dd}.";
                    _logger.LogWarning(msg);

                    // Example if you have custom ValidationException
                    throw new ApplicationException(msg);
                }
            } */

            // -------------------------------------------------
            // BUDGET VALIDATION (Optimistic - read-only check)
            // -------------------------------------------------
            if (dto.BudgetGroupId.HasValue && dto.BudgetGroupId.Value > 0)
            {
                // Use PO date as budget date (or null if default)
                DateOnly? budgetDate = null;
                if (dto.PODate != default)
                {
                    budgetDate = DateOnly.FromDateTime(dto.PODate.DateTime);
                }

                var remaining = await _budgetAllocationLookup.GetRemainingBalanceAsync(
                    budgetGroupId: dto.BudgetGroupId.Value,
                    budgetDate: budgetDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
                    monthId: dto.BudgetMonthId ?? 0,
                    requestById: dto.BudgetRequestById ?? 0,
                    projectId: dto.ProjectId,
                    wbsId: dto.WBSId,
                    financialYearId: dto.FinancialYearId,
                    ct: ct);

                var currentRemaining = remaining?.CurrentRemainingBalance ?? 0m;

                if (dto.PurchaseValue > currentRemaining)
                {
                    return new ApiResponseDTO<int>
                    {
                        IsSuccess = false,
                        Message = "Cannot create PO. Budget amount less than PO value. Budget Balance: " + currentRemaining,
                        Data = 0
                    };
                }
            }


            var entity = _mapper.Map<PurchaseOrderHeader>(dto);

            // --- Time zone
            var systemTimeZoneId = _timeZoneService.GetSystemTimeZone();
            TimeZoneInfo systemTimeZone;
            try
            {
                systemTimeZone = TimeZoneInfo.FindSystemTimeZoneById(systemTimeZoneId);
            }
            catch (TimeZoneNotFoundException)
            {
                systemTimeZone = string.Equals(systemTimeZoneId, "India Standard Time", StringComparison.OrdinalIgnoreCase)
                    ? TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata")
                    : TimeZoneInfo.Local;
            }
            var now = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, systemTimeZone);

            // --- Audit + numbering
            entity.UnitId = _ip.GetUnitId() ?? 0;
            

            // Determine transaction type based on PO category (Emergency overrides default)
            var poCategory = await _misc.GetByIdAsync(dto.POCategoryId);
            var isEmergency = string.Equals(poCategory?.Description, MiscEnumEntity.EmergencyPO, StringComparison.OrdinalIgnoreCase);
            var transactionTypeName = isEmergency
                ? MiscEnumEntity.TransactionTypeEPO
                : MiscEnumEntity.TransactionTypeLPO;

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

            // ---- Documents (rename on disk + attach to aggregate)
            await HandleDocumentsAsync(dto, entity, ct);

            // ════════════════════════════════════════════════════════════════════════════
            // ATOMIC TRANSACTION: PO Creation + Budget Allocation + Outbox Events
            // All operations participate in the SAME transaction with full rollback support.
            // If budget allocation fails -- entire transaction rolls back (PO + outbox events)
            // --------------------------------------------------------------

            var strategy = _repo.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                // Shared Transaction pattern: open the EF Core transaction and obtain the
                // underlying ADO.NET connection + transaction so the Budget Dapper UPDATE
                // participates in the same SQL transaction as the PO write.
                var (transaction, dbConn, dbTx) = await _repo.BeginTransactionWithConnectionAsync(ct);
                await using var _ = transaction;;

                try
                {
                    // --- Persist header + details (+ documents via graph)
                    var result = await _repo.CreateWithoutTransactionAsync(entity, ct);

                    if (result <= 0)
                    {
                        await transaction.RollbackAsync(ct);
                        return new ApiResponseDTO<int>
                        {
                            IsSuccess = false,
                            Message = "Purchase order was not created.",
                            Data = 0
                        };
                    }

                    if (entity.BudgetGroupId.HasValue && entity.BudgetGroupId.Value > 0 && entity.PurchaseValue > 0)
                    {
                        var budgetMonthDate = new DateOnly(entity.PODate.Year, entity.PODate.Month, 1);
                        var deltaApplied = await _budgetAllocationLookup.ApplyRemainingBalanceDeltaAsync(
                            budgetGroupId: entity.BudgetGroupId.Value,
                            budgetDate: budgetMonthDate,
                            monthId: entity.BudgetMonthId ?? 0,
                            requestById: entity.BudgetRequestById ?? 0,
                            deltaAmount: -entity.PurchaseValue,
                            projectId: entity.ProjectId,
                            wbsId: entity.WBSId,
                            financialYearId: entity.FinancialYearId,
                            connection: dbConn,
                            transaction: dbTx,
                            ct: ct);

                        if (!deltaApplied)
                        {
                            await transaction.RollbackAsync(ct);

                            _logger.LogError(
                                "Budget allocation failed. Rolling back PO creation. PO {PoId}, BG {BgId}, Delta {Delta}",
                                result,
                                entity.BudgetGroupId,
                                entity.PurchaseValue);

                            return new ApiResponseDTO<int>
                            {
                                IsSuccess = false,
                                Message = "Budget allocation failed. Purchase order creation rolled back.",
                                Data = 0

                            };
                        }
                    }

                    var workFlowEntity = await _repo.GetByIdPOLocalWorkFlowAsync(result);
                    var reversePayload = new CreatePOLocalReverseDto
                    {
                        Header = workFlowEntity,
                        Lines = null
                    };
                    var serializedPayload = JsonSerializer.Serialize(reversePayload);
                    var correlationId = Guid.NewGuid();

                    var workflowCommand = new CreateApprovalRequestCommand
                    {
                        CorrelationId = correlationId,
                        ModuleTypeName = MiscEnumEntity.POLocal,
                        ModuleTransactionId = result,
                        Payload = serializedPayload,
                        TransactionTypeId = transactionTypeId
                    };

                    /* var notifEventMisc = await _appDataMiscLookup.GetMiscMasterByNameAsync(
                        NotificationEnum.NotificationEvent, NotificationEnum.Create);

                    var notificationEvent = new NotificationCreatedEvent
                    {
                        CorrelationId = correlationId,
                        CreatedByName = entity.CreatedByName,
                        UnitId = _ip.GetUnitId() ?? 0,
                        ModuleName = "Purchase Order",
                        EventTypeId = notifEventMisc.Id,
                        param1 = entity.PONumber,
                        param2 = entity.CreatedByName,
                        param3 = entity.PODate,
                        param4 = entity.PurchaseValue.ToString(),
                        param5 = dto.VendorName,
                        ModuleTransactionId = result,
                        ModuleTypeName = MiscEnumEntity.POLocal
                    }; */

                    await _outboxEventPublisher.ScheduleWithoutSaveAsync(workflowCommand, correlationId, ct);
                 //   await _outboxEventPublisher.ScheduleWithoutSaveAsync(notificationEvent, correlationId, ct);

                    var impactedIndentIds = entity.Headers?
                        .SelectMany(h => h.Details ?? Enumerable.Empty<PurchaseLocalDetail>())
                        .Select(d => d.IndentId ?? 0)
                        .Where(id => id > 0)
                        .Distinct()
                        .ToHashSet() ?? new HashSet<int>();

                    if (impactedIndentIds.Count > 0)
                    {
                        await _repo.RecomputeIndentPoQtyAsync(impactedIndentIds, ct);
                    }

                    await _repo.SaveChangesAsync(ct);

                    // Increment DocNo via lookup — same connection + transaction
                    await _documentSequenceLookup.IncrementDocNoAsync(transactionTypeId, dbConn, dbTx);

                    await transaction.CommitAsync(ct);

                    _logger.LogInformation(
                        "PO {PoNumber} created with CorrelationId {CorrelationId}. Transaction committed. Events scheduled to outbox.",
                        entity.PONumber, correlationId);

                    return new ApiResponseDTO<int>
                    {
                        IsSuccess = true,
                        Message = "Purchase order created successfully.",
                        Data = result
                    };
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(ct);

                    _logger.LogError(ex,
                        "Exception during PO creation. Transaction rolled back. PONumber: {PONumber}",
                        entity.PONumber);

                    return new ApiResponseDTO<int>
                    {
                        IsSuccess = false,
                        Message = $"Purchase order creation failed: {ex.InnerException?.Message ?? ex.Message}",
                        Data = 0
                    };
                }
            });
        }
            
            
        private async Task HandleDocumentsAsync(
            LocalDtos.PurchaseOrderCreateDto dto,
            PurchaseOrderHeader entity,
            CancellationToken ct)
        {
            if (dto.Documents == null || !dto.Documents.Any())
                return;

            // Filter out dummy/placeholder docs
            dto.Documents = dto.Documents
                .Where(d => d.DocumentId != 0 &&
                            !string.Equals(d.FileName, "string", StringComparison.OrdinalIgnoreCase) &&
                            !string.IsNullOrWhiteSpace(d.FileName))
                .ToList();

            if (!dto.Documents.Any())
                return;

            // Base directory from DB
            var baseDirectory = await _poDocumentQueryRepository.GetBaseDirectoryAsync();
            if (string.IsNullOrWhiteSpace(baseDirectory))
            {
                _logger.LogError("Base directory for PO documents is not configured.");
                return;
            }

            // Resolve company & unit names
            var companyId = _ip.GetCompanyId() ?? 0;
            var unitId = _ip.GetUnitId() ?? 0;

            var units = await _unitLookup.GetAllUnitAsync();
            var companies = await _companyLookup.GetAllCompanyAsync();

            var unitLookupDict = units.ToDictionary(u => u.UnitId, u => u.UnitName);
            var companyLookupDict = companies.ToDictionary(c => c.CompanyId, c => c.CompanyName);

            companyLookupDict.TryGetValue(companyId, out var companyName);
            unitLookupDict.TryGetValue(unitId, out var unitName);

            companyName ??= string.Empty;
            unitName ??= string.Empty;

            // Build upload path: /Resources/{baseDirectory}/{companyName}/{unitName}
            var uploadPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "Resources",
                baseDirectory,
                companyName,
                unitName);

            EnsureDirectoryExists(uploadPath);

            foreach (var doc in dto.Documents)
            {
                if (string.IsNullOrWhiteSpace(doc.FileName))
                    continue;

                var oldFilePath = Path.Combine(uploadPath, doc.FileName);
                if (!File.Exists(oldFilePath))
                {
                    _logger.LogWarning("PO document temp file not found at path {Path}.", oldFilePath);
                    continue;
                }

                var newFileName = $"{entity.PONumber}_{doc.DocumentId}{Path.GetExtension(oldFilePath)}";
                var newFilePath = Path.Combine(uploadPath, newFileName);

                try
                {
                    File.Move(oldFilePath, newFilePath, overwrite: true);
                    doc.FileName = newFileName;

                    if (doc.UploadedDate == default)
                        doc.UploadedDate = DateTimeOffset.UtcNow;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Failed renaming PO document from {Old} to {New}.",
                        oldFilePath, newFilePath);
                }
            }

            // Attach to aggregate so EF can persist
            entity.PurchaseDocumentTypes = dto.Documents
                .Select(d => new PurchaseDocument
                {
                    DocumentId = d.DocumentId,
                    FileName = d.FileName,
                    UploadedDate = d.UploadedDate
                })
                .ToList();
        }

        private static void EnsureDirectoryExists(string path)
        {
            if (!string.IsNullOrWhiteSpace(path) && !Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
    }
}
