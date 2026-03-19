// ============================================================
// OLD CODE (used TransactionCreatedEvent — old event pattern)
// ============================================================
// using System.Text.Json;
// using AutoMapper;
// using Contracts.Events.Workflow;
// using Contracts.Common;
// using Contracts.Interfaces;
// using InventoryManagement.Application.Common.Interfaces;
// using InventoryManagement.Application.Common.Interfaces.IMRS;
// using InventoryManagement.Domain.Common;
// using InventoryManagement.Domain.Entities.MRS;
// using InventoryManagement.Domain.Events;
// using MediatR;
// using Microsoft.Extensions.Logging;
//
// namespace InventoryManagement.Application.MRS.Command.CreateMrsEntry
// {
//     public class CreateMrsEntryCommandHandler : IRequestHandler<CreateMrsEntryCommand, int>
//     {
//         public async Task<int> Handle(CreateMrsEntryCommand request, CancellationToken cancellationToken)
//         {
//             var mrsEntryHeader = _mapper.Map<MrsHeader>(request.MrsEntry);
//             if (string.IsNullOrWhiteSpace(mrsEntryHeader.MrsNo))
//             {
//                 mrsEntryHeader.MrsNo = await _iMrsEntryCommandRepository.GenerateNextCodeAsync();
//                 mrsEntryHeader.MrsDate = DateTime.Today;
//                 mrsEntryHeader.CreatedBy = _ipAddressService.GetUserId();
//                 mrsEntryHeader.CreatedDate = DateTime.Now;
//                 mrsEntryHeader.CreatedByName = _ipAddressService.GetUserName();
//                 mrsEntryHeader.CreatedIP = _ipAddressService.GetSystemIPAddress();
//             }
//             var result = await _iMrsEntryCommandRepository.CreateAsync(mrsEntryHeader);
//             var mrsReverseMap = _mapper.Map<MrsReverseMapDto>(result);
//             string serializedPayload = JsonSerializer.Serialize(mrsReverseMap);
//
//             if (result.Id > 0)
//             {
//                 var @event = new TransactionCreatedEvent          // ← OLD event
//                 {
//                     CorrelationId = Guid.NewGuid(),
//                     ModuleTypeName = MiscEnumEntity.MaterialRequest,
//                     ModuleTransactionId = result.Id,
//                     Payload = serializedPayload
//                 };
//                 await _eventPublisher.SaveEventAsync(@event);
//                 await _eventPublisher.PublishPendingEventsAsync();
//             }
//             await _mediator.Publish(new AuditLogsDomainEvent(...), cancellationToken);
//             return result.Id > 0 ? result.Id : throw new ExceptionRules("MRS Creation Failed.");
//         }
//     }
// }
// ============================================================
// NEW CODE (uses CreateApprovalRequestCommand — outbox pattern)
// ============================================================

using System.Text.Json;
using AutoMapper;
using Contracts.Commands.Workflow;
using Contracts.Common;
using Contracts.Interfaces;
using InventoryManagement.Application.Common.Interfaces.IOutbox;
using InventoryManagement.Application.Common.Interfaces.IMRS;
using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities.MRS;
using InventoryManagement.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace InventoryManagement.Application.MRS.Command.CreateMrsEntry
{
    public class CreateMrsEntryCommandHandler : IRequestHandler<CreateMrsEntryCommand, int>
    {
        private readonly IMrsEntryCommandRepository _iMrsEntryCommandRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IIPAddressService _ipAddressService;
        private readonly IOutboxEventPublisher _outboxEventPublisher;
        private readonly ILogger<CreateMrsEntryCommandHandler> _logger;

        public CreateMrsEntryCommandHandler(
            IMrsEntryCommandRepository iMrsEntryCommandRepository,
            IMapper mapper,
            IMediator mediator,
            IIPAddressService ipAddressService,
            IOutboxEventPublisher outboxEventPublisher,
            ILogger<CreateMrsEntryCommandHandler> logger)
        {
            _iMrsEntryCommandRepository = iMrsEntryCommandRepository;
            _mapper = mapper;
            _mediator = mediator;
            _ipAddressService = ipAddressService;
            _outboxEventPublisher = outboxEventPublisher;
            _logger = logger;
        }

        public async Task<int> Handle(CreateMrsEntryCommand request, CancellationToken cancellationToken)
        {
            var mrsEntryHeader = _mapper.Map<MrsHeader>(request.MrsEntry);

            // Auto-generate MrsNo if not set
            if (string.IsNullOrWhiteSpace(mrsEntryHeader.MrsNo))
            {
                mrsEntryHeader.MrsNo = await _iMrsEntryCommandRepository
                    .GenerateNextCodeAsync(cancellationToken);
                mrsEntryHeader.MrsDate = DateTime.Today;
                mrsEntryHeader.CreatedBy = _ipAddressService.GetUserId();
                mrsEntryHeader.CreatedDate = DateTime.Now;
                mrsEntryHeader.CreatedByName = _ipAddressService.GetUserName();
                mrsEntryHeader.CreatedIP = _ipAddressService.GetSystemIPAddress();
            }

            var result = await _iMrsEntryCommandRepository.CreateAsync(mrsEntryHeader);

            _logger.LogInformation("Create MRS. After Creation: {@result}", result);

            if (result.Id <= 0)
                throw new ExceptionRules("MRS Creation Failed.");

            // ---- SQL Transactional Outbox (same pattern as PartyMaster) ----
            var mrsReverseMap = _mapper.Map<MrsReverseMapDto>(result);
            var serializedPayload = JsonSerializer.Serialize(mrsReverseMap);

            var correlationId = Guid.NewGuid();
            var @event = new CreateApprovalRequestCommand
            {
                CorrelationId = correlationId,
                ModuleTypeName = MiscEnumEntity.MaterialRequest,
                ModuleTransactionId = result.Id,
                Payload = serializedPayload
            };

            await _outboxEventPublisher.ScheduleAsync(@event, correlationId, cancellationToken);

            // Audit log
            var evt = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "MRS_CREATE",
                actionName: result.MrsNo ?? result.Id.ToString(),
                details: $"MRS '{result.MrsNo}' created successfully with Id {result.Id}.",
                module: "MRSEntry"
            );
            await _mediator.Publish(evt, cancellationToken);

            return result.Id;
        }
    }
}
