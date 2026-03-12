// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text.Json;
// using System.Threading.Tasks;
// using AutoMapper;
// using Contracts.Events.Workflow;
// using Contracts.Common;
// using Contracts.Interfaces;
using PurchaseManagement.Application.Common.Interfaces;
// using PurchaseManagement.Application.Common.Interfaces.IIssueReturn;
// using PurchaseManagement.Application.Common.Interfaces.ILogService;
// using PurchaseManagement.Domain.Common;
// using PurchaseManagement.Domain.Entities.IssueReturn;
// using PurchaseManagement.Domain.Events;
// using MediatR;
// using Microsoft.Extensions.Logging;

// namespace PurchaseManagement.Application.IssueReturn.Command.CreateIssueReturn
// {
//     public class CreateIssueReturnEntryCommandHandler : IRequestHandler<CreateIssueReturnEntryCommand, int>
//     {
//         private readonly IIssueReturnEntryCommandRepository _iIssueReturnEntryCommandRepository;
//         private readonly IIssueReturnEntryQueryRepository _iIssueReturnQueryCommandRepository;
//         private readonly IMapper _mapper;
//         private readonly IMediator _mediator;
//         private readonly IIPAddressService _ipAddressService;
//         private readonly IEventPublisher _eventPublisher;
//         private readonly ILogServiceCommand _logServiceCommand;
//         private readonly ILogger<CreateIssueReturnEntryCommand> _logger;
        
//         public CreateIssueReturnEntryCommandHandler(IIssueReturnEntryCommandRepository iissueReturnEntryCommandRepository, IMapper mapper, IMediator mediator, IIssueReturnEntryQueryRepository iissueReturnQueryCommandRepository, IIPAddressService ipAddressService, IEventPublisher eventPublisher, ILogServiceCommand logServiceCommand, ILogger<CreateIssueReturnEntryCommand> logger)
//         {
//             _iIssueReturnEntryCommandRepository = iissueReturnEntryCommandRepository;
//             _mapper = mapper;
//             _mediator = mediator;
//             _iIssueReturnQueryCommandRepository = iissueReturnQueryCommandRepository;
//             _ipAddressService = ipAddressService;
//             _eventPublisher = eventPublisher;
//             _logServiceCommand = logServiceCommand;
//             _logger = logger;
//         }

//         public async Task<int> Handle(CreateIssueReturnEntryCommand request, CancellationToken cancellationToken)
//         {
//             var issueReturnHeader = _mapper.Map<IssueReturnHeader>(request.IssueReturnEntry);
//             // ✅ Auto-generate GateEntryNo if not set
//             if (string.IsNullOrWhiteSpace(issueReturnHeader.IssueReturnNo))
//             {
//                 issueReturnHeader.IssueReturnNo = await _iIssueReturnEntryCommandRepository
//                     .GenerateNextCodeAsync();  // Custom method for unique number
//                 issueReturnHeader.IssueReturnDate = DateTime.Today;
//                 issueReturnHeader.CreatedBy = _ipAddressService.GetUserId();
//                 issueReturnHeader.CreatedDate = DateTime.Now;
//                 issueReturnHeader.CreatedByName = _ipAddressService.GetUserName();
//                 issueReturnHeader.CreatedIP = _ipAddressService.GetSystemIPAddress();

//             }
//             var result = await _iIssueReturnEntryCommandRepository.CreateAsync(issueReturnHeader);
//             var issuereturnReverseMap = _mapper.Map<IssueReturnReverseMapDto>(result);

//             string serializedPayload = JsonSerializer.Serialize(issuereturnReverseMap);

//             _logger.LogInformation("Create Issue Return. After Creation: {@result}", result);
//              if (result.Id > 0 )
//             {
//                 var correlationId = Guid.NewGuid();
//                 var @event = new TransactionCreatedEvent
//                 {
//                     CorrelationId = correlationId,
//                     ModuleTypeName = MiscEnumEntity.IssueReturn,
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
//                 module: "IssueReturn"
//             );
//             await _mediator.Publish(evt, cancellationToken);
            
//             return result.Id > 0 ? result.Id : throw new ExceptionRules("Issue Return Creation Failed.");

//         }
//     }
// }