// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text;
// using System.Text.Json;
// using System.Text.Json.Serialization;
// using System.Threading.Tasks;
// using AutoMapper;
// using Contracts.Events.Workflow;
// using PurchaseManagement.Application.Common.Interfaces;
// using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.Local;
// using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ServicePO;
// using PurchaseManagement.Domain.Common;
// using PurchaseManagement.Domain.Entities.PurchaseOrder;
// using PurchaseManagement.Domain.Entities.PurchaseOrder.ServicePO;
// using PurchaseManagement.Domain.PurchaseOrder;
// using MediatR;
// using Microsoft.Extensions.Logging;

// namespace PurchaseManagement.Application.PurchaseOrder.ServicePO.Command.Create
// {
//     public class CreateServicePurchaseOrderCommandHandler : IRequestHandler<CreateServicePoCommand, int>
//     {

//         private readonly IMapper _mapper;
//         private readonly IServicePurchaseOrderCommandRepository _servicePurchaseOrderCommandRepository;
//         private readonly IServicePurchaseOrderQueryRepository _servicePurchaseOrderQueryRepository;
//         private readonly IIPAddressService _ip;
//         private readonly ITimeZoneService _timeZoneService;
//         private readonly IPurchaseOrderCommandRepository _repo;
//         private readonly IEventPublisher _eventPublisher;

//         private readonly ILogger<CreateServicePurchaseOrderCommandHandler> _logger;

//         public CreateServicePurchaseOrderCommandHandler(IMapper mapper, IServicePurchaseOrderCommandRepository servicePurchaseOrderCommandRepository, IIPAddressService ip, ITimeZoneService tz, IPurchaseOrderCommandRepository repo
//           , IEventPublisher eventPublisher, IServicePurchaseOrderQueryRepository poQuery, ILogger<CreateServicePurchaseOrderCommandHandler> logger)
//         {

//             _mapper = mapper;
//             _servicePurchaseOrderCommandRepository = servicePurchaseOrderCommandRepository;
//             _ip = ip;
//             _timeZoneService = tz;
//             _repo = repo;
//             _eventPublisher = eventPublisher;
//             _servicePurchaseOrderQueryRepository = poQuery;
//             _logger = logger;
//         }

//         public async Task<int> Handle(CreateServicePoCommand request, CancellationToken ct)
//         {
//             var dto = request.Data;
//             // 1) Map DTO -> aggregate
//             var entity = _mapper.Map<PurchaseOrderHeader>(request.Data);

//             // (Optional) auto-generate schedules if caller didn’t send any
//             // await ServicePoScheduleBuilder.EnsureSchedulesFromMiscAsync(entity, _misc, ct);

//             // 2) Audit + PO number
//             entity.UnitId = _ip.GetUnitId();
//             entity.PONumber = await _repo.GenerateNextCodeAsync(entity.POCategoryId, entity.POMethodId, entity.PODate, ct);
//             entity.CreatedBy = _ip.GetUserId();
//             entity.CreatedByName = _ip.GetUserName();
//             entity.CreatedIP = _ip.GetSystemIPAddress();
//             entity.CreatedDate = DateTimeOffset.UtcNow; // prefer UTC

//             // ---- 📎 Documents (rename on disk + attach to aggregate)
           
//             if (dto?.Documents != null && dto.Documents.Any())
//             {
//                 // Keep real docs only
//                 dto.Documents = dto.Documents
//                     .Where(d => d.DocumentId != 0 && !string.Equals(d.FileName, "string", StringComparison.OrdinalIgnoreCase))
//                     .ToList();

//                 if (dto.Documents.Any())
//                 {
//                     // Prefer central source for base directory; fallback to enum path if needed.
//                     var baseDirectory =MiscEnumEntity.DocumentPath;
//                     var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", baseDirectory);
//                     EnsureDirectoryExists(uploadPath);

//                     foreach (var doc in dto.Documents)
//                     {
//                         if (string.IsNullOrWhiteSpace(doc.FileName)) continue;

//                         var oldFilePath = Path.Combine(uploadPath, doc.FileName);
//                         if (!File.Exists(oldFilePath)) continue;

//                         var newFileName = $"{entity.PONumber}_{doc.DocumentId}{Path.GetExtension(oldFilePath)}";
//                         var newFilePath = Path.Combine(uploadPath, newFileName);

//                         try
//                         {
//                             File.Move(oldFilePath, newFilePath, overwrite: true);
//                             doc.FileName = newFileName;
//                             if (doc.UploadedDate == default) doc.UploadedDate = DateTimeOffset.UtcNow;
//                         }
//                         catch (Exception ex)
//                         {
//                             _logger.LogError(ex, "Failed renaming PO document to {NewFileName}", newFileName);
//                         }
//                     }

//                     // Attach to entity so EF persists the graph
//                     entity.PurchaseDocumentTypes = dto.Documents.Select(d => new PurchaseDocument
//                     {
//                         DocumentId = d.DocumentId,
//                         FileName = d.FileName,
//                         UploadedDate = d.UploadedDate
//                     }).ToList();
//                 }
//             }

//             // 3) Persist
//             var id = await _servicePurchaseOrderCommandRepository.CreateAsync(entity, ct);
//             if (id <= 0) return id;



//             // 4) Reload aggregate with real IDs for workflow packaging
//             var agg = await _servicePurchaseOrderQueryRepository.GetByIdAsync(id, ct)
//                     ?? throw new InvalidOperationException($"Service PO {id} not found after create.");


//             // 5) Build workflow payload
//             var wf = _mapper.Map<CreatePOServiceReverseDto>(agg);
//             wf.Lines = new();

//             // IMPORTANT: no camelCase policy => PascalCase keys (Header/UnitId) for your proc
//             var payload = JsonSerializer.Serialize(wf);
//             var payloadBytes = System.Text.Encoding.UTF8.GetByteCount(payload);



//             var correlationId = Guid.NewGuid();

//             var evt = new TransactionCreatedEvent
//             {
//                 CorrelationId = correlationId,
//                 ModuleTypeName = MiscEnumEntity.ServicePO, // must match consumer
//                 ModuleTransactionId = id,
//                 Payload = payload
//             };

//             // 6) Outbox save + publish (don’t fail the request if publish hiccups)
//             try
//             {
//                 await _eventPublisher.SaveEventAsync(evt);
//                 await _eventPublisher.PublishPendingEventsAsync();

//                 _logger.LogInformation(
//                     "ServicePO created. Id={PoId}, CorrelationId={CorrelationId}, PayloadBytes={Bytes}",
//                     id, correlationId, payload.Length);
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogWarning(ex,
//                     "ServicePO created but workflow publish deferred/failed. Id={PoId}, CorrelationId={CorrelationId}",
//                     id, correlationId);
//                 // Intentionally swallow to keep create successful; outbox can retry.
//             }

//             return id;
//         }
        
//         private static void EnsureDirectoryExists(string path)
//             {
//                 if (!string.IsNullOrWhiteSpace(path) && !Directory.Exists(path))
//                     Directory.CreateDirectory(path);
//             }
   
//     }
// }