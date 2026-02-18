// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text.Json;
// using System.Threading.Tasks;
// using AutoMapper;
// using Contracts.Events.Workflow;
// using Contracts.Interfaces.External.IWorkflow;
// using Contracts.Common;
// using PurchaseManagement.Application.Common.Interfaces;
// using PurchaseManagement.Application.Common.Interfaces.ILogService;
// using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
// using PurchaseManagement.Application.Common.Interfaces.IPurchaseIndent;
// using PurchaseManagement.Domain.Common;
// using PurchaseManagement.Domain.Entities;
// using PurchaseManagement.Domain.Events;
// using MediatR;
// using Microsoft.Extensions.Logging;

// namespace PurchaseManagement.Application.PurchaseIndents.Command.CreatePurchaseIndent
// {
//     public class CreatePurchaseIndentCommandHandler : IRequestHandler<CreatePurchaseIndentCommand, int>
//     {
//         private readonly IMapper _imapper;
//         private readonly IMediator _mediator;
//         private readonly IPurchaseIndentCommand _purchaseIndentCommand;
//         private readonly ILogServiceCommand _logServiceCommand;
//         private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
//         private readonly IPurchaseIndentQuery _purchaseIndentQuery;
//         private readonly IEventPublisher _eventPublisher;
//         private readonly ILogger<CreatePurchaseIndentCommandHandler> _logger;

//         public CreatePurchaseIndentCommandHandler(IPurchaseIndentCommand purchaseIndentCommand, IMapper imapper,
//         IMediator mediator, ILogServiceCommand logServiceCommand, IMiscMasterQueryRepository miscMasterQueryRepository, IPurchaseIndentQuery purchaseIndentQuery,
//         IEventPublisher eventPublisher, ILogger<CreatePurchaseIndentCommandHandler> logger)
//         {
//             _purchaseIndentCommand = purchaseIndentCommand;
//             _imapper = imapper;
//             _mediator = mediator;
//             _logServiceCommand = logServiceCommand;
//             _miscMasterQueryRepository = miscMasterQueryRepository;
//             _purchaseIndentQuery = purchaseIndentQuery;
//             _eventPublisher = eventPublisher;
//             _logger = logger;
//         }
//         public async Task<int> Handle(CreatePurchaseIndentCommand request, CancellationToken cancellationToken)
//         {
//             _logger.LogInformation("Create Purchase Indent. Before Creation: {@request}", request);
//             var Indent = _imapper.Map<IndentHeader>(request);

//             var IndentNumber = await _purchaseIndentQuery.GeneratePurchaseIndentNumberAsync(request.UnitId);
//             Indent.IndentNumber = IndentNumber;

//              var StatusMisc = await _miscMasterQueryRepository.GetMiscMasterByName(MiscEnumEntity.Status, MiscEnumEntity.Draft);
//             var StatusPending = await _miscMasterQueryRepository.GetMiscMasterByName(MiscEnumEntity.Status, MiscEnumEntity.Pending);

//             Indent.StatusId = request.IsDraft == 1 ? StatusMisc.Id : StatusPending.Id;

//             foreach (var item in Indent.IndentDetails)
//             {
//                 item.StatusId = request.IsDraft == 1 ? StatusMisc.Id : StatusPending.Id;
//             }
            
//             var result = await _purchaseIndentCommand.CreateAsync(Indent);           

//             var indentReverseMap = _imapper.Map<IndentReverseMapDto>(result);

//             string serializedPayload = JsonSerializer.Serialize(indentReverseMap);

//             _logger.LogInformation("Create Purchase Indent. After Creation: {@result}", result);
//             // var IndentLog = new IndentLog
//             // {
//             //     IndentHeaderId = result.Id,
//             //     ActionType = "Created",
//             //     ActionRemarks = "Indent Created",
//             //     NewData = serializedPayload,
//             //     StatusId = request.IsDraft == 1 ? StatusMisc.Id : StatusPending.Id
//             // };

//             //     await _logServiceCommand.CreateAsync(IndentLog);

//             if (result.Id > 0 && request.IsDraft ==0)
//             {
//                 var correlationId = Guid.NewGuid();
//                 var @event = new TransactionCreatedEvent
//                 {
//                     CorrelationId = correlationId,
//                     ModuleTypeName = MiscEnumEntity.PurchaseIndent,
//                     ModuleTransactionId = result.Id,
//                     Payload = serializedPayload
//                 };

//                 await _eventPublisher.SaveEventAsync(@event);
//                 await _eventPublisher.PublishPendingEventsAsync();
//             }
//               var evt = new AuditLogsDomainEvent(
//                 actionDetail: "Create",
//                 actionCode: "Create",
//                 actionName: "Create",
//                 details: JsonSerializer.Serialize(request),
//                 module: "PurchaseIndent"
//             );
//             await _mediator.Publish(evt, cancellationToken);
            
//             return result.Id > 0 ? result.Id : throw new ExceptionRules("Indent Creation Failed.");
//         }
//     }
// }