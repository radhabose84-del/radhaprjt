using System.Text.Json;
using AutoMapper;
using Contracts.Events.Notifications;
using Contracts.Events.Workflow;
using Contracts.Interfaces.Lookups.Budget;
using Contracts.Interfaces.Lookups.Users;
using PurchaseManagement.Application.Common.HttpResponse;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IPurchaseDocument;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.Local;
using PurchaseManagement.Application.PurchaseOrder.Dtos.Local;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.PurchaseOrder;
using PurchaseManagement.Domain.PurchaseOrder;
using MediatR;
using Microsoft.Extensions.Logging;

namespace PurchaseManagement.Application.PurchaseOrder.Local.Commands.Create
{
    public class CreatePurchaseOrderCommandHandler
        : IRequestHandler<CreatePurchaseOrderCommand, ApiResponseDTO<int>>
    {
        private readonly IPurchaseOrderCommandRepository _repo;
        private readonly IPurchaseOrderQueryRepository _purchaseOrderQueryRepository;
        private readonly IMapper _mapper;
        private readonly IIPAddressService _ip;
        private readonly ITimeZoneService _timeZoneService;
        private readonly ILogger<CreatePurchaseOrderCommandHandler> _logger;
        //private readonly IEventPublisher _eventPublisher;
        private readonly IPODocumentQueryRepository _poDocumentQueryRepository;
        private readonly IUnitLookup _unitLookup;
        private readonly ICompanyLookup _companyLookup;
        private readonly IBudgetAllocationLookup _budgetAllocationLookup;
        private readonly IFinancialYearLookup _financialYearLookup;

        public CreatePurchaseOrderCommandHandler(
            IPurchaseOrderCommandRepository repo,
            IMapper mapper,
            IIPAddressService ip,
            ITimeZoneService timeZoneService,
            ILogger<CreatePurchaseOrderCommandHandler> logger,
            //IEventPublisher eventPublisher,
            IPurchaseOrderQueryRepository purchaseOrderQueryRepository,
            IPODocumentQueryRepository poDocumentQueryRepository,
            IUnitLookup unitLookup,
            ICompanyLookup companyLookup,
            IBudgetAllocationLookup budgetAllocationLookup,
            IFinancialYearLookup financialYearLookup)
        {
            _repo = repo;
            _mapper = mapper;
            _ip = ip ?? throw new ArgumentNullException(nameof(ip));
            _timeZoneService = timeZoneService;
            _logger = logger;
            //_eventPublisher = eventPublisher;
            _purchaseOrderQueryRepository = purchaseOrderQueryRepository;
            _poDocumentQueryRepository = poDocumentQueryRepository;
            _unitLookup = unitLookup;
            _companyLookup = companyLookup;
            _budgetAllocationLookup = budgetAllocationLookup ??
                                      throw new ArgumentNullException(nameof(budgetAllocationLookup));
            _financialYearLookup = financialYearLookup;
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

            if (request.Data.FinancialYearId <= 0 || request.Data.FinancialYearId == null)
            {
                var financialYears = await _financialYearLookup
                    .GetAllFinancialYearAsync();

                // convert DateTimeOffset → DateOnly for comparison
                var fyMatch = financialYears
                    .FirstOrDefault(fy =>
                    {
                        var start = DateOnly.FromDateTime(fy.StartDate.Date);
                        var end   = DateOnly.FromDateTime(fy.EndDate.Date);
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
            }

             // -------------------------------------------------
            // 🔍 BUDGET VALIDATION (BudgetGroup vs PurchaseValue)
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
                        Message = "Cannot create PO. Budget amount less than PO value.Budget Balance : " + currentRemaining,
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
            entity.UnitId = _ip.GetUnitId();

            // Get unit code for PO number generation
            var units = await _unitLookup.GetAllUnitAsync();
            var unitLookupDict = units.ToDictionary(u => u.UnitId, u => (u.ShortName ?? string.Empty).Trim());
            if (!unitLookupDict.TryGetValue(entity.UnitId, out var unitCode) || string.IsNullOrWhiteSpace(unitCode))
            {
                return new ApiResponseDTO<int>
                {
                    IsSuccess = false,
                    Message = $"Invalid UnitId {entity.UnitId}. Failed to generate PO number.",
                    Data = 0
                };
            }

            entity.PONumber = await _repo.GenerateNextCodeAsync(
                entity.POCategoryId,
                entity.POMethodId,
                entity.PODate,
                unitCode,
                ct);

            entity.CreatedBy = _ip.GetUserId();
            entity.CreatedByName = _ip.GetUserName();
            entity.CreatedIP = _ip.GetSystemIPAddress();
            entity.CreatedDate = now;

            // ---- 📎 Documents (rename on disk + attach to aggregate)
            await HandleDocumentsAsync(dto, entity, ct);

            // --- Persist header + details (+ documents via graph)
            var result = await _repo.CreateAsync(entity, ct);

            if (result > 0)
            {
                // ---- Apply budget allocation delta after successful PO creation
                if (entity.BudgetGroupId.HasValue && entity.BudgetGroupId.Value > 0 && entity.PurchaseValue > 0)
                {
                    var budgetMonthDate = new DateOnly(entity.PODate.Year, entity.PODate.Month, 1);
                    var deltaApplied = await _budgetAllocationLookup.ApplyRemainingBalanceDeltaAsync(
                        budgetGroupId: entity.BudgetGroupId.Value,
                        budgetDate: budgetMonthDate,
                        monthId: entity.BudgetMonthId ?? 0,
                        requestById: entity.BudgetRequestById ?? 0,
                        deltaAmount: entity.PurchaseValue,
                        projectId: entity.ProjectId,
                        wbsId: entity.WBSId,
                        financialYearId: entity.FinancialYearId,
                        ct: ct);

                    if (!deltaApplied)
                    {
                        _logger.LogWarning(
                            "ApplyRemainingBalanceDeltaAsync (create) failed. PO {PoId}, BG {BgId}, Delta {Delta}",
                            result,
                            entity.BudgetGroupId,
                            entity.PurchaseValue);
                    }
                }

                // ---- Reload aggregate with details for payload
             /*    var agg = await _purchaseOrderQueryRepository.GetByIdAsync(result, ct)
                          ?? throw new InvalidOperationException($"Purchase Order - Local {result} not found after create.");

                // ---- Map to payload and publish workflow + notification events
                var reversePayload = _mapper.Map<CreatePOLocalReverseDto>(agg);
                var serializedPayload = JsonSerializer.Serialize(reversePayload);
                var correlationId = Guid.NewGuid();

                var evt = new TransactionCreatedEvent
                {
                    CorrelationId = correlationId,
                    ModuleTypeName = MiscEnumEntity.POLocal,
                    ModuleTransactionId = result,
                    Payload = serializedPayload
                };

                var notification = new NotificationCreatedEvent
                {
                    CorrelationId = correlationId,
                    CreatedByName = entity.CreatedByName,
                    UnitId = _ip.GetUnitId(),
                    ModuleName = "Purchase Order",
                    EventTypeId = (int)NotificationEnum.NotificationEvent.Create,
                    param1 = entity.PONumber,
                    param2 = entity.CreatedByName,
                    param3 = entity.PODate,
                    param4 = entity.PurchaseValue.ToString(),
                    param5 = dto.VendorName
                };

                await _eventPublisher.SaveEventAsync(evt);
                await _eventPublisher.SaveEventAsync(notification);
                await _eventPublisher.PublishPendingEventsAsync(); */

                return new ApiResponseDTO<int>
                {
                    IsSuccess = true,
                    Message = "Purchase order created successfully.",
                    Data = result
                };
            }

            return new ApiResponseDTO<int>
            {
                IsSuccess = false,
                Message = "Purchase order was not created.",
                Data = 0
            };
        }

        private async Task HandleDocumentsAsync(
            PurchaseOrderCreateDto dto,
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
            var companyId = _ip.GetCompanyId();
            var unitId = _ip.GetUnitId();

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

        private static void EnsureDirectoryExists(string? path)
        {
            if (!string.IsNullOrWhiteSpace(path) && !Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
    }
}
