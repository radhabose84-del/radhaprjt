// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.IO;
// using System.Text.Json;
// using System.Threading.Tasks;
// using AutoMapper;
// using Contracts.Common;
// using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ServicePO;
// using PurchaseManagement.Domain.Entities.PurchaseOrder.ServicePO;
// using PurchaseManagement.Domain.Events;
// using MediatR;
// using ServiceEntrySheetEntity   = PurchaseManagement.Domain.Entities.PurchaseOrder.ServicePO.ServiceEntrySheet;
// using ServiceEntryActivityEntity = PurchaseManagement.Domain.Entities.PurchaseOrder.ServicePO.ServiceEntryActivity;
// using Contracts.Interfaces;
using PurchaseManagement.Application.Common.Interfaces;
// using System.Text;
// using Contracts.Events.Workflow;
// using PurchaseManagement.Domain.Common;
// using Microsoft.Extensions.Logging;
// using PurchaseManagement.Domain.PurchaseOrder;

// namespace PurchaseManagement.Application.PurchaseOrder.ServicePO.Command.CreateServiceEntrySheet
// {
//     public class CreateServiceEntrySheetCommandHandler : IRequestHandler<CreateServiceEntrySheetCommand, int>
//     {
//         private readonly IServicePurchaseOrderCommandRepository _servicePurchaseOrderCommandRepository;
//         private readonly IServicePurchaseOrderQueryRepository _servicePurchaseOrderQueryRepository;
//         private readonly IMapper _mapper;
//         private readonly IMediator _mediator;
//         private readonly IIPAddressService _ip;
//         private readonly ITimeZoneService _timeZoneService;        
//         private readonly IEventPublisher _eventPublisher;
//         private readonly ILogger<CreateServiceEntrySheetCommandHandler> _logger;

//         public CreateServiceEntrySheetCommandHandler(IServicePurchaseOrderCommandRepository servicePurchaseOrderCommandRepository, IServicePurchaseOrderQueryRepository poQuery, IMapper mapper, IMediator mediator
//          , IIPAddressService ip, ITimeZoneService tz, IEventPublisher eventPublisher, ILogger<CreateServiceEntrySheetCommandHandler> logger)
//         {
//             _servicePurchaseOrderCommandRepository = servicePurchaseOrderCommandRepository;
//             _servicePurchaseOrderQueryRepository = poQuery;
//             _mapper = mapper;
//             _mediator = mediator;
//             _ip = ip;
//             _timeZoneService = tz;
//             _eventPublisher = eventPublisher;
//             _logger = logger;

//         }
        
//         public async Task<int> Handle(CreateServiceEntrySheetCommand request, CancellationToken ct)
//         {

           
//             var dto = request.CreateServiceSheet ?? throw new ExceptionRules("Payload is required.");

//             // (A) Verify PO is a valid Service PO for SES
//             var po = await _servicePurchaseOrderQueryRepository.GetServicePOHeaderForSesAsync(dto.PurchaseOrderId, ct);
//             if (po is null)
//                 throw new ExceptionRules($"PO {dto.PurchaseOrderId} not found or not a Service PO.");
//             if (po.VendorId != dto.VendorId)
//                 throw new ExceptionRules($"Vendor mismatch. PO vendor={po.VendorId}, payload vendor={dto.VendorId}.");            

//             // (B) Map DTO -> entity
//             var ses = _mapper.Map<ServiceEntrySheetEntity>(dto);

//             // Audit fields from IP service
//             ses.UnitId = _ip.GetUnitId() ?? 0;
//             ses.CreatedBy = _ip.GetUserId();
//             ses.CreatedByName = _ip.GetUserName();
//             ses.CreatedIP = _ip.GetSystemIPAddress();
//             ses.CreatedDate = DateTimeOffset.UtcNow;

//             // Ensure child collection not null
//             ses.Activities ??= new List<ServiceEntryActivityEntity>();

//             // (C) Compute totals
//             decimal qty      = dto.ActualQuantity ?? 0m;
//             decimal rate     = dto.ActualRate     ?? 0m;
//             decimal baseVal  = dto.ActualValue    ?? decimal.Round(qty * rate, 2, MidpointRounding.AwayFromZero);

//             decimal discount = dto.DiscountValue  ?? 0m;
//             decimal taxable  = baseVal - discount;
//             if (taxable < 0m) taxable = 0m;

//             decimal taxPct   = dto.TaxPercentage  ?? 0m;
//             decimal taxVal   = dto.TaxValue       ?? decimal.Round(taxable * (taxPct / 100m), 2, MidpointRounding.AwayFromZero);
//             decimal totalVal = dto.TotalValue     ?? decimal.Round(taxable + taxVal, 2, MidpointRounding.AwayFromZero);

//             ses.ActualValue = baseVal;
//             ses.TaxValue    = taxVal;
//             ses.TotalValue  = totalVal;

//             // (Optional) initial status = Pending
//             // var pending = await _misc.GetMiscMasterByName(MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Pending);
//             // ses.StatusId = pending.Id;

//               // ---- 📎 Documents (rename on disk + attach to aggregate)
           
//             if (dto?.ServiceEntryDocumentDtos  != null && dto.ServiceEntryDocumentDtos .Any())
//             {
//                 // Keep real docs only
//                 dto.ServiceEntryDocumentDtos  = dto.ServiceEntryDocumentDtos 
//                     .Where(d => d.DocumentId != 0 && !string.Equals(d.FileName, "string", StringComparison.OrdinalIgnoreCase))
//                     .ToList();

//                 if (dto.ServiceEntryDocumentDtos .Any())
//                 {
//                     // Prefer central source for base directory; fallback to enum path if needed.
//                     var baseDirectory =MiscEnumEntity.DocumentPath;
//                     var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", baseDirectory);
//                     EnsureDirectoryExists(uploadPath);

//                     foreach (var doc in dto.ServiceEntryDocumentDtos )
//                     {
//                         if (string.IsNullOrWhiteSpace(doc.FileName)) continue;

//                         var oldFilePath = Path.Combine(uploadPath, doc.FileName);
//                         if (!File.Exists(oldFilePath)) continue;

//                         var sesDate = dto.SESDate == default
//                             ? DateTimeOffset.UtcNow
//                             : dto.SESDate;

//                         var timeStamp = sesDate.ToString("yyyyMMdd'T'HHmmss");

//                         var newFileName =
//                             $"SES_{dto.PurchaseOrderId}_{dto.ScheduleID}_{dto.OccurrenceNo}_{doc.DocumentId}_{timeStamp}{Path.GetExtension(oldFilePath)}";

//                        // var newFileName = $"SES_{dto.PurchaseOrderId}_{doc.DocumentId}{Path.GetExtension(oldFilePath)}";
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
//                     ses.Documents = dto.ServiceEntryDocumentDtos.Select(d => new ServiceEntrySheetDocument
//                     {
//                         DocumentId = d.DocumentId,
//                         FileName = d.FileName,
//                         UploadedDate = d.UploadedDate
//                     }).ToList();
//                 }
//             }


//             // (D) Persist
//             var saved = await _servicePurchaseOrderCommandRepository.CreateServiceEntrySheetAsync(ses, ct);
//             if (saved.Id <= 0)
//                 throw new ExceptionRules("Service Entry Sheet creation failed.");

//             // (E1) Audit log
//             await _mediator.Publish(new AuditLogsDomainEvent(
//                 actionDetail: "Create",
//                 actionCode: "CreateSES",
//                 actionName: "ServiceEntrySheet",
//                 details: JsonSerializer.Serialize(new { SESId = saved.Id, PO = saved.PurchaseOrderId }),
//                 module: "ServiceEntry"), ct);

//             // (E2) Reload full SES for workflow payload
//             var agg = await _servicePurchaseOrderCommandRepository.GetServiceEntrySheetByIdAsync(saved.Id, ct)
//                       ?? throw new InvalidOperationException($"Service Entry Sheet {saved.Id} not found after create.");

           
 

//             // (E3) Build workflow payload (same pattern as ServicePO)
//             var wf = _mapper.Map<CreateServiceEntrySheetWorkflowDto>(agg);

//             var payload = JsonSerializer.Serialize(wf);              // PascalCase by default
//             var payloadBytes = Encoding.UTF8.GetByteCount(payload);

//             var correlationId = Guid.NewGuid();

//             var evt = new TransactionCreatedEvent
//             {
//                 CorrelationId = correlationId,
//                 ModuleTypeName = MiscEnumEntity.ServiceEntrySheet,  
//                 ModuleTransactionId = saved.Id,
//                 Payload = payload
//             };

//             // (E4) Outbox save + publish (non-blocking)
//             try
//             {
//                 await _eventPublisher.SaveEventAsync(evt);
//                 await _eventPublisher.PublishPendingEventsAsync();

//                 _logger.LogInformation(
//                     "ServiceEntrySheet created. Id={SesId}, CorrelationId={CorrelationId}, PayloadBytes={Bytes}",
//                     saved.Id, correlationId, payloadBytes);
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogWarning(ex,
//                     "ServiceEntrySheet created but workflow publish deferred/failed. Id={SesId}, CorrelationId={CorrelationId}",
//                     saved.Id, correlationId);
//             }

//             return saved.Id;
//         }
        
     
//         private static void EnsureDirectoryExists(string path)
//             {
//                 if (!string.IsNullOrWhiteSpace(path) && !Directory.Exists(path))
//                     Directory.CreateDirectory(path);
//             }

//     }
// }