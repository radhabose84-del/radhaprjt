// using System.Text.Json;
// using AutoMapper;
// using Contracts.Events.Notifications;
// using Contracts.Events.Workflow;
// using PurchaseManagement.Application.Common.Interfaces;
// using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
// using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ImportPO;
// using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IPurchaseDocument;
// using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.Local;
// using PurchaseManagement.Domain.Common;
// using PurchaseManagement.Domain.Entities.PurchaseOrder;
// using PurchaseManagement.Domain.PurchaseOrder;
// using MediatR;
// using Microsoft.Extensions.Logging;

// namespace PurchaseManagement.Application.PurchaseOrder.ImportPO.Command.Create;

// public class CreateImportPOCommandHandler : IRequestHandler<CreateImportPOCommand, int>
// {
//     private readonly IImportPOCommandRepository _repo;
//     private readonly IIPAddressService _ip;
//     private readonly ITimeZoneService _tz;
//     private readonly IMapper _mapper;
//     private readonly IPurchaseOrderCommandRepository _poRepo;
//     private readonly ILogger<CreateImportPOCommandHandler> _logger;
//     private readonly IMiscMasterQueryRepository _misc;
//     private readonly IPODocumentQueryRepository _poDocumentQueryRepository;
//     private readonly IImportPOQueryRepository _purchaseOrderQueryRepository;    
//     private readonly IEventPublisher _eventPublisher;

//     public CreateImportPOCommandHandler(
//         IImportPOCommandRepository repo,
//         IMapper mapper,
//         IIPAddressService ip,
//         ITimeZoneService tz,
//         ILogger<CreateImportPOCommandHandler> logger,
//         IMiscMasterQueryRepository misc,
//         IPurchaseOrderCommandRepository poRepo,
//         IPODocumentQueryRepository poDocumentQueryRepository,  IImportPOQueryRepository purchaseOrderQueryRepository,IEventPublisher eventPublisher)
//     {
//         _repo = repo; _mapper = mapper; _ip = ip; _tz = tz; _logger = logger;
//         _misc = misc; _poRepo = poRepo; _poDocumentQueryRepository = poDocumentQueryRepository;_purchaseOrderQueryRepository = purchaseOrderQueryRepository; _eventPublisher = eventPublisher;
//     }

//     public async Task<int> Handle(CreateImportPOCommand request, CancellationToken ct)
//     {
//         var dto = request.Data;
//         var entity = _mapper.Map<PurchaseOrderHeader>(dto);

//         // audit
//         var tzId = _tz.GetSystemTimeZone();
//         TimeZoneInfo tzi;
//         try { tzi = TimeZoneInfo.FindSystemTimeZoneById(tzId); }
//         catch { tzi = TimeZoneInfo.Local; }
//         var now = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, tzi);

//         entity.UnitId = _ip.GetUnitId();
//         entity.PONumber = await _poRepo.GenerateNextCodeAsync(entity.POCategoryId, entity.POMethodId, entity.PODate, ct);
//         entity.CreatedBy = _ip.GetUserId();
//         entity.CreatedByName = _ip.GetUserName();
//         entity.CreatedIP = _ip.GetSystemIPAddress();
//         entity.CreatedDate = now;

//         // 🔐 Documents: filter + rename on disk, then attach to entity.PurchaseDocuments
//         if (dto.Documents != null && dto.Documents.Any())
//         {
//             dto.Documents = dto.Documents
//                 .Where(d => d.DocumentId != 0 && !string.Equals(d.FileName, "string", StringComparison.OrdinalIgnoreCase))
//                 .ToList();

//             if (dto.Documents.Any())
//             {
//                 var baseDirectory = MiscEnumEntity.DocumentPath; // or await _poDocumentQueryRepository.GetBaseDirectoryAsync();
//                 var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", baseDirectory);
//                 EnsureDirectoryExists(uploadPath);

//                 foreach (var doc in dto.Documents)
//                 {
//                     if (string.IsNullOrWhiteSpace(doc.FileName)) continue;

//                     var oldFilePath = Path.Combine(uploadPath, doc.FileName);
//                     if (File.Exists(oldFilePath))
//                     {
//                         var newFileName = $"{entity.PONumber}_{doc.DocumentId}{Path.GetExtension(oldFilePath)}";
//                         var newFilePath = Path.Combine(uploadPath, newFileName);
//                         File.Move(oldFilePath, newFilePath, overwrite: true);
//                         doc.FileName = newFileName;
//                         if (doc.UploadedDate == default) doc.UploadedDate = DateTimeOffset.UtcNow;
//                     }
//                 }
                
//                 // ✅ Attach to aggregate so EF saves with the graph
//                 entity.PurchaseDocumentTypes = dto.Documents.Select(d => new PurchaseDocument
//                 {
//                     DocumentId = d.DocumentId,
//                     FileName = d.FileName,
//                     UploadedDate = d.UploadedDate
//                 }).ToList();
//             }
//         }

//         var result = await _repo.CreateAsync(entity, dto, ct);
//         if (result > 0)
//         {
//             // ---- Reload aggregate with details for payload
//             var agg = await _purchaseOrderQueryRepository.GetByIdAsync(result, ct)
//                       ?? throw new InvalidOperationException($"Purchase Order - Import {result} not found after create.");

//             // ---- Map to payload and publish workflow event
//             var reversePayload = _mapper.Map<CreateImportPOReverseDto>(agg);
//             var serializedPayload = JsonSerializer.Serialize(reversePayload);
//             var correlationId = Guid.NewGuid();

//             var evt = new TransactionCreatedEvent
//             {
//                 CorrelationId = correlationId,
//                 ModuleTypeName = MiscEnumEntity.POLocal,
//                 ModuleTransactionId = result,
//                 Payload = serializedPayload
//             };

//             var notification = new NotificationCreatedEvent
//             {
//                 CorrelationId = correlationId,
//                 CreatedByName = entity.CreatedByName,
//                 UnitId = _ip.GetUnitId(),
//                 ModuleName = "Purchase Order",
//                 EventTypeId = (int)NotificationEnum.NotificationEvent.Create,
//                 param1 = entity.PONumber,
//                 param2 = entity.PONumber,
//                 param3 = entity.PODate,
//                 param4 = entity.PurchaseValue.ToString()
//             };

//             await _eventPublisher.SaveEventAsync(evt);
//             await _eventPublisher.SaveEventAsync(notification);
//             await _eventPublisher.PublishPendingEventsAsync();
//         }

//         return result;
//     }

//     private static void EnsureDirectoryExists(string path)
//     {
//         if (!string.IsNullOrWhiteSpace(path) && !Directory.Exists(path))
//             Directory.CreateDirectory(path);
//     }
// }