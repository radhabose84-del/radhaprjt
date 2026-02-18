// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text.Json;
// using System.Threading.Tasks;
// using AutoMapper;
// using Contracts.Events.Workflow;
// using Contracts.Common;
// using InventoryManagement.Application.Common.Interfaces;
// using InventoryManagement.Application.Common.Interfaces.IMRS;
// using InventoryManagement.Domain.Common;
// using InventoryManagement.Domain.Entities.MRS;
// using InventoryManagement.Domain.Events;
// using MediatR;
// using Microsoft.Extensions.Logging;

// namespace InventoryManagement.Application.MRS.Command.CreateMrsEntry
// {
//     public class CreateMrsEntryCommandHandler : IRequestHandler<CreateMrsEntryCommand, int>
//     {
//         private readonly IMrsEntryCommandRepository _iMrsEntryCommandRepository;
//         private readonly IMrsEntryQueryRepository _iMrsEntryQueryRepository;
//         private readonly IMapper _mapper;
//         private readonly IMediator _mediator;
//         private readonly IIPAddressService _ipAddressService;
//         private readonly IEventPublisher _eventPublisher;
//         private readonly ILogger<CreateMrsEntryCommandHandler> _logger;
//           public CreateMrsEntryCommandHandler(IMrsEntryCommandRepository iMrsEntryCommandRepository, IMapper mapper, IMediator mediator, IMrsEntryQueryRepository iMrsEntryQueryRepository, IIPAddressService ipAddressService, IEventPublisher eventPublisher, ILogger<CreateMrsEntryCommandHandler> logger)
//         {
//             _iMrsEntryCommandRepository = iMrsEntryCommandRepository;
//             _mapper = mapper;
//             _mediator = mediator;
//             _iMrsEntryQueryRepository = iMrsEntryQueryRepository;
//             _ipAddressService = ipAddressService;
//             _eventPublisher = eventPublisher;
//             _logger = logger;
//         }

//         public async Task<int> Handle(CreateMrsEntryCommand request, CancellationToken cancellationToken)
//         {
//             var mrsEntryHeader = _mapper.Map<MrsHeader>(request.MrsEntry);
//             // ✅ Auto-generate GateEntryNo if not set
//             if (string.IsNullOrWhiteSpace(mrsEntryHeader.MrsNo))
//             {
//                 mrsEntryHeader.MrsNo = await _iMrsEntryCommandRepository
//                     .GenerateNextCodeAsync();  // Custom method for unique number
//                 mrsEntryHeader.MrsDate = DateTime.Today;
//                 mrsEntryHeader.CreatedBy = _ipAddressService.GetUserId();
//                 mrsEntryHeader.CreatedDate = DateTime.Now;
//                 mrsEntryHeader.CreatedByName = _ipAddressService.GetUserName();
//                 mrsEntryHeader.CreatedIP = _ipAddressService.GetSystemIPAddress();

//             }
//              var result = await _iMrsEntryCommandRepository.CreateAsync(mrsEntryHeader);
             
//             var mrsReverseMap = _mapper.Map<MrsReverseMapDto>(result);

//             string serializedPayload = JsonSerializer.Serialize(mrsReverseMap);

//             _logger.LogInformation("Create MRS. After Creation: {@result}", result);
            

//              if (result.Id > 0 )
//             {
//                 var correlationId = Guid.NewGuid();
//                 var @event = new TransactionCreatedEvent
//                 {
//                     CorrelationId = correlationId,
//                     ModuleTypeName = MiscEnumEntity.MaterialRequest,
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
//                 module: "MRSEntry"
//             );
//             await _mediator.Publish(evt, cancellationToken);
            
//             return result.Id > 0 ? result.Id : throw new ExceptionRules("MRS Creation Failed.");

            
//         }
//     }
// }