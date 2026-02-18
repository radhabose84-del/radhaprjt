// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text.Json;
// using System.Threading.Tasks;
// using AutoMapper;
// using Contracts.Events.Workflow;
// using Contracts.Common;
// using PurchaseManagement.Application.Common.Interfaces;
// using PurchaseManagement.Application.Common.Interfaces.ILogService;
// using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
// using PurchaseManagement.Application.Common.Interfaces.IPurchaseIndent;
// using PurchaseManagement.Application.PurchaseIndents.Command.CreatePurchaseIndent;
// using PurchaseManagement.Domain.Common;
// using PurchaseManagement.Domain.Entities;
// using PurchaseManagement.Domain.Events;
// using MediatR;
// using Microsoft.Extensions.Logging;

// namespace PurchaseManagement.Application.PurchaseIndents.Command.UpdatePurchaseIndent
// {
//     public class UpdatePurchaseIndentCommandHandler : IRequestHandler<UpdatePurchaseIndentCommand, bool>
//     {
//         private readonly IPurchaseIndentCommand _purchaseIndentCommand;
//         private readonly IMediator _imediator;
//         private readonly IMapper _imapper;
//         private readonly IEventPublisher _eventPublisher;
//         private readonly IPurchaseIndentQuery _purchaseIndentQuery;
//         private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
//         private readonly ILogger<UpdatePurchaseIndentCommandHandler> _logger;
//         public UpdatePurchaseIndentCommandHandler(IPurchaseIndentCommand purchaseIndentCommand, IMediator imediator, IMapper imapper, IEventPublisher eventPublisher,
//          IPurchaseIndentQuery purchaseIndentQuery, IMiscMasterQueryRepository miscMasterQueryRepository, ILogger<UpdatePurchaseIndentCommandHandler> logger)
//         {
//             _purchaseIndentCommand = purchaseIndentCommand;
//             _imediator = imediator;
//             _imapper = imapper;
//             _eventPublisher = eventPublisher;
//             _purchaseIndentQuery = purchaseIndentQuery;
//             _miscMasterQueryRepository = miscMasterQueryRepository;
//             _logger = logger;
//         }
//         public async Task<bool> Handle(UpdatePurchaseIndentCommand request, CancellationToken cancellationToken)
//         {
            
//             var Indent = _imapper.Map<IndentHeader>(request);

//               var StatusMisc = await _miscMasterQueryRepository.GetMiscMasterByName(MiscEnumEntity.Status, MiscEnumEntity.Draft);
//             var StatusPending = await _miscMasterQueryRepository.GetMiscMasterByName(MiscEnumEntity.Status, MiscEnumEntity.Pending);

//             Indent.StatusId = request.IsDraft == 1 ? StatusMisc.Id : StatusPending.Id;

//             foreach (var item in Indent.IndentDetails)
//             {
//                 item.StatusId = request.IsDraft == 1 ? StatusMisc.Id : StatusPending.Id;
//             }
            
//             var CheckPending = await _purchaseIndentQuery.GetByIdAsync(request.Id);
//             var result = await _purchaseIndentCommand.UpdateAsync(Indent,JsonSerializer.Serialize(request));

//             var indentData = await _purchaseIndentQuery.GetByIdAsync(request.Id);
            
//             var indentReverseMap = _imapper.Map<IndentReverseMapDto>(indentData);

//             string serializedPayload = JsonSerializer.Serialize(indentReverseMap);

//             _logger.LogInformation("Update Purchase Indent TransactionCreatedEvent Publish check. {result},{IsDraft},{StatusMisc},{CheckPending}",
//              result,request.IsDraft,StatusMisc.Id,CheckPending.StatusId);
//             // if (result && request.IsDraft == 0 && StatusMisc.Id == CheckPending.StatusId)
//             if (result && request.IsDraft == 0)
//             {
//                 var correlationId = Guid.NewGuid();
//                 var @event = new TransactionCreatedEvent
//                 {
//                     CorrelationId = correlationId,
//                     ModuleTypeName = MiscEnumEntity.PurchaseIndent,
//                     ModuleTransactionId = request.Id,
//                     Payload = serializedPayload
//                 };

//                 await _eventPublisher.SaveEventAsync(@event);
//                 await _eventPublisher.PublishPendingEventsAsync();
//             }
//              var evt = new AuditLogsDomainEvent(
//                 actionDetail: "Update",
//                 actionCode: "Update",
//                 actionName: "Update",
//                 details: JsonSerializer.Serialize(request),
//                 module: "PurchaseIndent"
//             );
//             await _imediator.Publish(evt, cancellationToken);
//             return result == true ? result : throw new ExceptionRules("Indent update failed."); 
//         }
//     }
// }