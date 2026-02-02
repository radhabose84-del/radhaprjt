// using System.Text.Json;
// using AutoMapper;
// using Contracts.Events.Notifications;
// using Contracts.Events.Workflow;
// using Contracts.Interfaces.External.IBudget;
// using Contracts.Interfaces.External.IUser;
// using PurchaseManagement.Application.Common.HttpResponse;
// using PurchaseManagement.Application.Common.Interfaces;
// using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IPurchaseDocument;
// using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.Local;
// using PurchaseManagement.Application.PurchaseOrder.Dtos.Local;
// using PurchaseManagement.Domain.Common;
// using PurchaseManagement.Domain.Entities.PurchaseOrder;
// using PurchaseManagement.Domain.PurchaseOrder;
// using MediatR;
// using Microsoft.Extensions.Logging;

// namespace PurchaseManagement.Application.PurchaseOrder.Local.Commands.Create
// {
//     public class CreatePurchaseOrderCommandHandler
//         : IRequestHandler<CreatePurchaseOrderCommand, ApiResponseDTO<int>>
//     {
//         private readonly IPurchaseOrderCommandRepository _repo;
//         private readonly IPurchaseOrderQueryRepository _purchaseOrderQueryRepository;
//         private readonly IMapper _mapper;
//         private readonly IIPAddressService _ip;
//         private readonly ITimeZoneService _timeZoneService;
//         private readonly ILogger<CreatePurchaseOrderCommandHandler> _logger;
//         private readonly IEventPublisher _eventPublisher;
//         private readonly IPODocumentQueryRepository _poDocumentQueryRepository;
//         private readonly IUnitGrpcClient _unitGrpcClient;
//         private readonly ICompanyGrpcClient _companyGrpcClient;
//         private readonly IBudgetAllocationGrpcClient _budgetAllocationGrpcClient; 
//         private readonly IFinancialYearGrpcClient _financialYearGrpcClient;

//         public CreatePurchaseOrderCommandHandler(
//             IPurchaseOrderCommandRepository repo,
//             IMapper mapper,
//             IIPAddressService ip,
//             ITimeZoneService timeZoneService,
//             ILogger<CreatePurchaseOrderCommandHandler> logger,
//             IEventPublisher eventPublisher,
//             IPurchaseOrderQueryRepository purchaseOrderQueryRepository,
//             IPODocumentQueryRepository poDocumentQueryRepository,
//             IUnitGrpcClient unitGrpcClient,
//             ICompanyGrpcClient companyGrpcClient,IBudgetAllocationGrpcClient budgetAllocationGrpcClient,IFinancialYearGrpcClient financialYearGrpcClient)
//         {
//             _repo = repo;
//             _mapper = mapper;
//             _ip = ip ?? throw new ArgumentNullException(nameof(ip));
//             _timeZoneService = timeZoneService;
//             _logger = logger;
//             _eventPublisher = eventPublisher;
//             _purchaseOrderQueryRepository = purchaseOrderQueryRepository;
//             _poDocumentQueryRepository = poDocumentQueryRepository;
//             _unitGrpcClient = unitGrpcClient;
//             _companyGrpcClient = companyGrpcClient;
//              _budgetAllocationGrpcClient = budgetAllocationGrpcClient ?? 
//                                            throw new ArgumentNullException(nameof(budgetAllocationGrpcClient)); _financialYearGrpcClient = financialYearGrpcClient;
//         }

//         public async Task<ApiResponseDTO<int>> Handle(CreatePurchaseOrderCommand request, CancellationToken ct)
//         {
//             var dto = request.Data;
//             if (dto == null)
//             {
//                 return new ApiResponseDTO<int>
//                 {
//                     IsSuccess = false,
//                     Message = "Invalid request payload.",
//                     Data = 0
//                 };
//             }            
            
//             var requestDate = DateOnly.FromDateTime(request.Data.PODate.DateTime);

//             if (request.Data.FinancialYearId <= 0 || request.Data.FinancialYearId == null)
//             {
//                 var financialYears = await _financialYearGrpcClient
//                     .GetAllFinancialYearAsync(ct);

//                 // convert DateTimeOffset → DateOnly for comparison
//                 var fyMatch = financialYears
//                     .FirstOrDefault(fy =>
//                     {
//                         var start = DateOnly.FromDateTime(fy.StartDate.Date);
//                         var end   = DateOnly.FromDateTime(fy.EndDate.Date);
//                         return start <= requestDate && requestDate <= end;
//                     });

//                 if (fyMatch != null)
//                 {
//                     request.Data.FinancialYearId = fyMatch.Id;                    
//                 }
//                 else
//                 {
//                     var msg = $"Financial year does not exist for date {requestDate:yyyy-MM-dd}.";
//                     _logger.LogWarning(msg);

//                     // Example if you have custom ValidationException
//                     throw new ApplicationException(msg);
//                 }
//             }

//              // -------------------------------------------------
//             // 🔍 BUDGET VALIDATION (BudgetGroup vs PurchaseValue)
//             // -------------------------------------------------
//             if (dto.BudgetGroupId.HasValue && dto.BudgetGroupId.Value > 0)
//             {
//                 // Use PO date as budget date (or null if default)
//                 DateOnly? budgetDate = null;
//                 if (dto.PODate != default)
//                 {
//                     budgetDate = DateOnly.FromDateTime(dto.PODate.DateTime);
//                 }

//                 var remaining = await _budgetAllocationGrpcClient.GetRemainingBalanceAsync(
//                     budgetGroupId: dto.BudgetGroupId.Value,
//                     budgetDate: budgetDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
//                     monthId: dto.BudgetMonthId ?? 0,
//                     requestById: dto.BudgetRequestById ?? 0,
//                     projectId: dto.ProjectId,
//                     wbsId: dto.WBSId,
//                     financialYearId: dto.FinancialYearId,
//                     ct: ct);

//                 var currentRemaining = remaining?.CurrentRemainingBalance ?? 0m;

//                 if (dto.PurchaseValue > currentRemaining)
//                 {
//                     return new ApiResponseDTO<int>
//                     {
//                         IsSuccess = false,
//                         Message = "Cannot create PO. Budget amount less than PO value.Budget Balance : " + currentRemaining,
//                         Data = 0
//                     };
//                 }
//             }


//             var entity = _mapper.Map<PurchaseOrderHeader>(dto);

//             // --- Time zone
//             var systemTimeZoneId = _timeZoneService.GetSystemTimeZone();
//             TimeZoneInfo systemTimeZone;
//             try
//             {
//                 systemTimeZone = TimeZoneInfo.FindSystemTimeZoneById(systemTimeZoneId);
//             }
//             catch (TimeZoneNotFoundException)
//             {
//                 systemTimeZone = string.Equals(systemTimeZoneId, "India Standard Time", StringComparison.OrdinalIgnoreCase)
//                     ? TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata")
//                     : TimeZoneInfo.Local;
//             }
//             var now = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, systemTimeZone);

//             // --- Audit + numbering
//             entity.UnitId = _ip.GetUnitId();
//             entity.PONumber = await _repo.GenerateNextCodeAsync(
//                 entity.POCategoryId,
//                 entity.POMethodId,
//                 entity.PODate,
//                 ct);

//             entity.CreatedBy = _ip.GetUserId();
//             entity.CreatedByName = _ip.GetUserName();
//             entity.CreatedIP = _ip.GetSystemIPAddress();
//             entity.CreatedDate = now;

//             // ---- 📎 Documents (rename on disk + attach to aggregate)
//             await HandleDocumentsAsync(dto, entity, ct);

//             // --- Persist header + details (+ documents via graph)
//             var result = await _repo.CreateAsync(entity, ct);

//             if (result > 0)
//             {
//                 // ---- Reload aggregate with details for payload
//                 var agg = await _purchaseOrderQueryRepository.GetByIdAsync(result, ct)
//                           ?? throw new InvalidOperationException($"Purchase Order - Local {result} not found after create.");

//                 // ---- Map to payload and publish workflow + notification events
//                 var reversePayload = _mapper.Map<CreatePOLocalReverseDto>(agg);
//                 var serializedPayload = JsonSerializer.Serialize(reversePayload);
//                 var correlationId = Guid.NewGuid();

//                 var evt = new TransactionCreatedEvent
//                 {
//                     CorrelationId = correlationId,
//                     ModuleTypeName = MiscEnumEntity.POLocal,
//                     ModuleTransactionId = result,
//                     Payload = serializedPayload
//                 };

//                 var notification = new NotificationCreatedEvent
//                 {
//                     CorrelationId = correlationId,
//                     CreatedByName = entity.CreatedByName,
//                     UnitId = _ip.GetUnitId(),
//                     ModuleName = "Purchase Order",
//                     EventTypeId = (int)NotificationEnum.NotificationEvent.Create,
//                     param1 = entity.PONumber,
//                     param2 = entity.CreatedByName,
//                     param3 = entity.PODate,
//                     param4 = entity.PurchaseValue.ToString(),
//                     param5 = dto.VendorName
//                 };

//                 await _eventPublisher.SaveEventAsync(evt);
//                 await _eventPublisher.SaveEventAsync(notification);
//                 await _eventPublisher.PublishPendingEventsAsync();

//                 return new ApiResponseDTO<int>
//                 {
//                     IsSuccess = true,
//                     Message = "Purchase order created successfully.",
//                     Data = result
//                 };
//             }

//             return new ApiResponseDTO<int>
//             {
//                 IsSuccess = false,
//                 Message = "Purchase order was not created.",
//                 Data = 0
//             };
//         }

//         private async Task HandleDocumentsAsync(
//             PurchaseOrderCreateDto dto,
//             PurchaseOrderHeader entity,
//             CancellationToken ct)
//         {
//             if (dto.Documents == null || !dto.Documents.Any())
//                 return;

//             // Filter out dummy/placeholder docs
//             dto.Documents = dto.Documents
//                 .Where(d => d.DocumentId != 0 &&
//                             !string.Equals(d.FileName, "string", StringComparison.OrdinalIgnoreCase) &&
//                             !string.IsNullOrWhiteSpace(d.FileName))
//                 .ToList();

//             if (!dto.Documents.Any())
//                 return;

//             // Base directory from DB
//             var baseDirectory = await _poDocumentQueryRepository.GetBaseDirectoryAsync();
//             if (string.IsNullOrWhiteSpace(baseDirectory))
//             {
//                 _logger.LogError("Base directory for PO documents is not configured.");
//                 return;
//             }

//             // Resolve company & unit names
//             var companyId = _ip.GetCompanyId();
//             var unitId = _ip.GetUnitId();

//             var units = await _unitGrpcClient.GetAllUnitAsync();
//             var companies = await _companyGrpcClient.GetAllCompanyAsync();

//             var unitLookup = units.ToDictionary(u => u.UnitId, u => u.UnitName);
//             var companyLookup = companies.ToDictionary(c => c.CompanyId, c => c.CompanyName);

//             companyLookup.TryGetValue(companyId, out var companyName);
//             unitLookup.TryGetValue(unitId, out var unitName);

//             companyName ??= string.Empty;
//             unitName ??= string.Empty;

//             // Build upload path: /Resources/{baseDirectory}/{companyName}/{unitName}
//             var uploadPath = Path.Combine(
//                 Directory.GetCurrentDirectory(),
//                 "Resources",
//                 baseDirectory,
//                 companyName,
//                 unitName);

//             EnsureDirectoryExists(uploadPath);

//             foreach (var doc in dto.Documents)
//             {
//                 if (string.IsNullOrWhiteSpace(doc.FileName))
//                     continue;

//                 var oldFilePath = Path.Combine(uploadPath, doc.FileName);
//                 if (!File.Exists(oldFilePath))
//                 {
//                     _logger.LogWarning("PO document temp file not found at path {Path}.", oldFilePath);
//                     continue;
//                 }

//                 var newFileName = $"{entity.PONumber}_{doc.DocumentId}{Path.GetExtension(oldFilePath)}";
//                 var newFilePath = Path.Combine(uploadPath, newFileName);

//                 try
//                 {
//                     File.Move(oldFilePath, newFilePath, overwrite: true);
//                     doc.FileName = newFileName;

//                     if (doc.UploadedDate == default)
//                         doc.UploadedDate = DateTimeOffset.UtcNow;
//                 }
//                 catch (Exception ex)
//                 {
//                     _logger.LogError(ex,
//                         "Failed renaming PO document from {Old} to {New}.",
//                         oldFilePath, newFilePath);
//                 }
//             }

//             // Attach to aggregate so EF can persist
//             entity.PurchaseDocumentTypes = dto.Documents
//                 .Select(d => new PurchaseDocument
//                 {
//                     DocumentId = d.DocumentId,
//                     FileName = d.FileName,
//                     UploadedDate = d.UploadedDate
//                 })
//                 .ToList();
//         }

//         private static void EnsureDirectoryExists(string? path)
//         {
//             if (!string.IsNullOrWhiteSpace(path) && !Directory.Exists(path))
//                 Directory.CreateDirectory(path);
//         }
//     }
// }
