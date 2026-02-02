// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text.Json;
// using System.Threading.Tasks;
// using AutoMapper;
// using Contracts.Events.Workflow;
// using PurchaseManagement.Application.Common.Interfaces;
// using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
// using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ServicePO;
// using PurchaseManagement.Application.PurchaseOrder.ServicePO.Command.Create;
// using PurchaseManagement.Domain.Common;
// using PurchaseManagement.Domain.Entities.PurchaseOrder;
// using PurchaseManagement.Domain.PurchaseOrder;
// using MediatR;
// using Microsoft.Extensions.Logging;

// namespace PurchaseManagement.Application.PurchaseOrder.SevicePOAmendment
// {
//     public class SevicePOAmendmentCommandHandler : IRequestHandler<SevicePOAmendmentCommand, int>
//     {

//         private readonly IServicePurchaseOrderQueryRepository _qry;
//         private readonly IServicePurchaseOrderCommandRepository _cmd;
//         private readonly IMiscMasterQueryRepository _misc;
//         private readonly IIPAddressService _ip;
//         private readonly ITimeZoneService _tz;
//         private readonly IMapper _mapper;
//         private readonly ILogger<SevicePOAmendmentCommandHandler> _logger;
//          private readonly IEventPublisher _eventPublisher;


//         public SevicePOAmendmentCommandHandler(IServicePurchaseOrderQueryRepository qry, IServicePurchaseOrderCommandRepository cmd,
//             IMiscMasterQueryRepository misc, IMapper mapper, IIPAddressService ip, ITimeZoneService tz, ILogger<SevicePOAmendmentCommandHandler> logger, IEventPublisher eventPublisher)
//         {
//             _qry = qry;
//             _cmd = cmd;
//             _misc = misc;
//             _mapper = mapper;
//             _ip = ip;
//             _tz = tz;
//             _logger = logger;
//             _eventPublisher = eventPublisher;
//         }

//         public async Task<int> Handle(SevicePOAmendmentCommand request, CancellationToken ct)
//         {
//             var poId = request.Dto.Id;
//             if (poId <= 0)
//                 throw new InvalidOperationException("Invalid Service PO Id for amendment.");

//             // 1️⃣ Guard: existing DTO
//             var existingDto = await _qry.GetServicePOByIdAsync(poId, ct)
//                              ?? throw new InvalidOperationException($"Service PO {poId} not found.");

//             // 2️⃣ Only approved can be amended
//             var approved = await _misc.GetMiscMasterByName(MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Approved);
//             if (existingDto.StatusId != approved.Id)
//                 throw new InvalidOperationException("PO is not approved. Use Edit for Pending POs; Amendment is only for Approved POs.");

//             // 3️⃣ Load existing entity (for EF-based AmendAsync)
//             var existingEntity = await _cmd.GetAggregateAsync(poId, ct)
//                                  ?? throw new InvalidOperationException($"Service PO entity {poId} not found.");

//             // 4️⃣ Timezone + now
//             var tzId = _tz.GetSystemTimeZone();
//             if (string.IsNullOrWhiteSpace(tzId) || tzId.Equals("India Standard Time", StringComparison.OrdinalIgnoreCase))
//                 tzId = "Asia/Kolkata";

//             TimeZoneInfo tzi;
//             try { tzi = TimeZoneInfo.FindSystemTimeZoneById(tzId); }
//             catch { tzi = TimeZoneInfo.Local; }

//             var now = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, tzi);

//             // 5️⃣ Normalize revised DTO: revision, PONumber, remarks
//             var revisedDto = request.Dto;
//             revisedDto.RevisionNo = existingEntity.RevisionNo + 1;
//             revisedDto.PONumber = BuildNextRevisionCode(existingEntity.PONumber, revisedDto.RevisionNo);
//             revisedDto.AmendmentReason = (request.Dto.AmendmentReason ?? revisedDto.AmendmentReason)?.Trim();

//             // 6️⃣ Document handling
//             if (revisedDto.Documents is { Count: > 0 })
//             {
//                 var baseDir = "ServicePoDocument"; // or take from Misc if needed
//                 var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "Resources", baseDir);
//                 if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

//                 foreach (var doc in revisedDto.Documents
//                              .Where(d => d.DocumentId > 0 && !string.IsNullOrWhiteSpace(d.FileName)))
//                 {
//                     var oldPath = Path.Combine(uploadDir, doc.FileName!);
//                     if (File.Exists(oldPath))
//                     {
//                         var finalName = $"{revisedDto.PONumber}_{doc.DocumentId}{Path.GetExtension(oldPath)}";
//                         var newPath = Path.Combine(uploadDir, finalName);

//                         if (!string.Equals(oldPath, newPath, StringComparison.OrdinalIgnoreCase))
//                         {
//                             try
//                             {
//                                 File.Move(oldPath, newPath, overwrite: true);
//                                 doc.FileName = finalName;
//                             }
//                             catch (Exception ex)
//                             {
//                                 _logger.LogError(ex,
//                                     "Failed renaming Service PO document to {FinalName} for PO {PoNumber}",
//                                     finalName, revisedDto.PONumber);
//                             }
//                         }
//                     }

//                     if (doc.UploadedDate == default)
//                         doc.UploadedDate = now;
//                 }
//             }

//             // 7️⃣ Clear IDs so new rows are inserted for amended PO
//             if (revisedDto.ServicePos != null)
//             {
//                 foreach (var sh in revisedDto.ServicePos)
//                 {
//                     sh.Id = null;
//                     if (sh.Lines == null) continue;

//                     foreach (var ln in sh.Lines)
//                     {
//                         ln.Id = null;
//                         if (ln.Schedules == null) continue;

//                         foreach (var sc in ln.Schedules)
//                             sc.Id = null;
//                     }
//                 }
//             }

//             if (revisedDto.PaymentTerms != null)
//             {
//                 foreach (var t in revisedDto.PaymentTerms)
//                     t.Id = null;
//             }

//             // 8️⃣ Map DTO → entity
//             var revisedEntity = _mapper.Map<PurchaseOrderHeader>(revisedDto);

//             // Map docs to entity for EF
//             if (revisedDto.Documents is { Count: > 0 })
//             {
//                 revisedEntity.PurchaseDocumentTypes = revisedDto.Documents
//                     .Where(d => d.DocumentId > 0 && !string.IsNullOrWhiteSpace(d.FileName))
//                     .Select(d => new PurchaseDocument
//                     {
//                         DocumentId = d.DocumentId,
//                         FileName = d.FileName,
//                         UploadedDate = d.UploadedDate
//                     })
//                     .ToList();
//             }
//             else
//             {
//                 revisedEntity.PurchaseDocumentTypes = new List<PurchaseDocument>();
//             }

//             // 9️⃣ Amend in repo
//             var newId = await _cmd.AmendAsync(existingEntity, revisedEntity, ct);

//             _logger.LogInformation("Service PO amended. OldId={OldId}, NewId={NewId}", poId, newId);
            
//             _logger.LogInformation("Service PO amended. OldId={OldId}, NewId={NewId}", poId, newId);

//             // 🔟 Workflow trigger (same pattern as Create Service PO)
//             try
//             {
//                 // Load fresh aggregate for workflow payload
//                 var wfAgg = await _cmd.GetAggregateAsync(newId, ct)
//                              ?? throw new InvalidOperationException($"Service PO entity {newId} not found after amend.");

//                 // Map to workflow DTO
//                 var wf = _mapper.Map<CreatePOServiceReverseDto>(wfAgg);
//                 // If your workflow expects only Header (no lines), uncomment:
//                 // wf.Lines = new();

//                 var payload = JsonSerializer.Serialize(wf);
//                 var correlationId = Guid.NewGuid();

//                 var evt = new TransactionCreatedEvent
//                 {
//                     CorrelationId = correlationId,
//                     ModuleTypeName = MiscEnumEntity.ServicePO, // must match your consumer
//                     ModuleTransactionId = newId,
//                     Payload = payload
//                 };

//                 await _eventPublisher.SaveEventAsync(evt);
//                 await _eventPublisher.PublishPendingEventsAsync();

//                 _logger.LogInformation(
//                     "ServicePO amendment workflow triggered. OldId={OldId}, NewId={NewId}, CorrelationId={CorrelationId}",
//                     poId, newId, correlationId);
//             }
//             catch (Exception ex)
//             {
//                 // Keep amendment successful; outbox or background worker can retry publish.
//                 _logger.LogWarning(ex,
//                     "ServicePO amended but workflow publish failed/deferred. OldId={OldId}, NewId={NewId}",
//                     poId, newId);
//             }
 



//             return newId;
//         }
//          private static string BuildNextRevisionCode(string oldCode, int nextRev)
//         {
//             var idx = oldCode.LastIndexOf("-R", StringComparison.OrdinalIgnoreCase);
//             if (idx >= 0 && idx + 2 < oldCode.Length && int.TryParse(oldCode[(idx + 2)..], out _))
//                 return oldCode[..idx] + $"-R{nextRev}";
//             return $"{oldCode}-R{nextRev}";
//         }
//         // public async Task<int> Handle(SevicePOAmendmentCommand request, CancellationToken ct)
//         // {
//         //     var poId = request.Dto.Id;
//         //     if (poId <= 0) throw new InvalidOperationException("Invalid Service PO Id for amendment.");

//         //     // Read DTO for guards
//         //     var existingDto = await _qry.GetServicePOByIdAsync(poId, ct)
//         //                     ?? throw new InvalidOperationException($"Service PO {poId} not found.");

//         //     // Approved only
//         //     var approved = await _misc.GetMiscMasterByName(MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Approved);
//         //     if (existingDto.StatusId != approved.Id)
//         //         throw new InvalidOperationException("PO is not approved. Use Edit for Pending POs; Amendment is only for Approved POs.");

//         //     // Load EXISTING ENTITY for EF repo
//         //     var existingEntity = await _cmd.GetAggregateAsync(poId, ct)
//         //                         ?? throw new InvalidOperationException($"Service PO entity {poId} not found.");

//         //     // 4️⃣ Timezone + now (for UploadedDate etc.)
//         //     var tzId = _tz.GetSystemTimeZone();
//         //     if (string.IsNullOrWhiteSpace(tzId) || tzId.Equals("India Standard Time", StringComparison.OrdinalIgnoreCase))
//         //         tzId = "Asia/Kolkata";

//         //     TimeZoneInfo tzi;
//         //     try { tzi = TimeZoneInfo.FindSystemTimeZoneById(tzId); }
//         //     catch { tzi = TimeZoneInfo.Local; }

//         //     var now = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, tzi);

//         //     // 5️⃣ Normalize revised DTO – revision, PO number, remarks
//         //     var revisedDto = request.Dto;

//         //     // Compute next revision & new PONumber similar to Local PO amendment
//         //     revisedDto.RevisionNo = existingEntity.RevisionNo + 1;
//         //     revisedDto.PONumber = BuildNextRevisionCode(existingEntity.PONumber, revisedDto.RevisionNo);
//         //     revisedDto.AmendmentReason = (request.Dto.AmendmentReason ?? revisedDto.AmendmentReason)?.Trim();

//         //     // 6️⃣ Normalize documents (same pattern as Local PO amendment)
//         //     if (revisedDto.Documents is { Count: > 0 })
//         //     {
//         //         // You can use a constant or MiscEnumEntity.DocumentPath for this
//         //         var baseDir = "ServicePoDocument";
//         //         var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "Resources", baseDir);
//         //         if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

//         //         foreach (var doc in revisedDto.Documents
//         //                      .Where(d => d.DocumentId > 0 && !string.IsNullOrWhiteSpace(d.FileName)))
//         //         {
//         //             var oldPath = Path.Combine(uploadDir, doc.FileName!);
//         //             if (File.Exists(oldPath))
//         //             {
//         //                 var finalName = $"{revisedDto.PONumber}_{doc.DocumentId}{Path.GetExtension(oldPath)}";
//         //                 var newPath = Path.Combine(uploadDir, finalName);

//         //                 if (!string.Equals(oldPath, newPath, StringComparison.OrdinalIgnoreCase))
//         //                 {
//         //                     File.Move(oldPath, newPath, overwrite: true);
//         //                     doc.FileName = finalName; // update DTO so entity gets correct name
//         //                 }
//         //             }

//         //             if (doc.UploadedDate == default)
//         //                 doc.UploadedDate = now;
//         //         }
//         //     }


//         //     // // Normalize revised DTO (clear ids so EF inserts)
//         //     // var revisedDto = request.Dto;
//         //     // revisedDto.AmendmentReason = (request.Dto.AmendmentReason ?? revisedDto.AmendmentReason)?.Trim();

//         //     foreach (var sh in revisedDto.ServicePos)
//         //     {
//         //         sh.Id = null;
//         //         foreach (var ln in sh.Lines)
//         //         {
//         //             ln.Id = null;
//         //             foreach (var sc in ln.Schedules) sc.Id = null;
//         //         }
//         //     }
//         //     foreach (var t in revisedDto.PaymentTerms) t.Id = null;

//         //     // Map DTO → ENTITY
//         //     var revisedEntity = _mapper.Map<PurchaseOrderHeader>(revisedDto);


//         //     // Map docs from DTO to entity navigation so EF can persist Purchase.PurchaseDocuments
//         //     if (revisedDto.Documents is { Count: > 0 })
//         //     {
//         //         revisedEntity.PurchaseDocumentTypes = revisedDto.Documents
//         //             .Where(d => d.DocumentId > 0 && !string.IsNullOrWhiteSpace(d.FileName))
//         //             .Select(d => new PurchaseDocument
//         //             {
//         //                 DocumentId   = d.DocumentId,
//         //                 FileName     = d.FileName,
//         //                 UploadedDate = d.UploadedDate
//         //                 // PoId will be set by relationship when saving
//         //             })
//         //             .ToList();
//         //     }
//         //     else
//         //     {
//         //         revisedEntity.PurchaseDocumentTypes = new List<PurchaseDocument>();
//         //     }

//         //     // Call EF-style repo (entities in)
//         //     var newId = await _cmd.AmendAsync(existingEntity, revisedEntity, ct);

//         //     _logger.LogInformation("Service PO amended. OldId={OldId}, NewId={NewId}", poId, newId);
//         //     return newId;
//         // }




//     }
// }