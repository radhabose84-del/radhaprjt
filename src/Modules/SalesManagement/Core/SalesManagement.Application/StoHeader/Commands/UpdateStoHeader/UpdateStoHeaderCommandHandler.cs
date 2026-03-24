using AutoMapper;
using Contracts.Commands.Workflow;
using Contracts.Common;
using Contracts.Interfaces;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IOutbox;
using SalesManagement.Application.Common.Interfaces.IStoHeader;
using SalesManagement.Domain.Common;
using SalesManagement.Domain.Events;
using System.Text.Json;

namespace SalesManagement.Application.StoHeader.Commands.UpdateStoHeader
{
    public class UpdateStoHeaderCommandHandler : IRequestHandler<UpdateStoHeaderCommand, ApiResponseDTO<int>>
    {
        private readonly IStoHeaderCommandRepository _commandRepository;
        private readonly IStoHeaderQueryRepository _queryRepository;
        private readonly IOutboxEventPublisher _outboxEventPublisher;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateStoHeaderCommandHandler(
            IStoHeaderCommandRepository commandRepository,
            IStoHeaderQueryRepository queryRepository,
            IOutboxEventPublisher outboxEventPublisher,
            IIPAddressService ipAddressService,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _outboxEventPublisher = outboxEventPublisher;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateStoHeaderCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.StoHeader>(request);

            var result = await _commandRepository.UpdateAsync(entity);

            // Publish workflow approval request via Outbox
            var unitId = _ipAddressService.GetUnitId();
            var correlationId = Guid.NewGuid();
            var payload = JsonSerializer.Serialize(new
            {
                Header = new
                {
                    Id = request.Id,
                    StoNumber = (string?)null,
                    StoTypeId = request.StoTypeId,
                    UnitId = unitId ?? 0,
                    StatusId = entity.HeaderStatusId
                },
                Lines = new List<object>()
            });

            var workflowEvent = new CreateApprovalRequestCommand
            {
                CorrelationId = correlationId,
                ModuleTypeName = MiscEnumEntity.StoModuleTypeName,
                ModuleTransactionId = request.Id,
                Payload = payload
            };
            await _outboxEventPublisher.ScheduleAsync(workflowEvent, correlationId, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "STO_HEADER_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Stock Transfer Order with Id {request.Id} updated successfully.",
                module: "StoHeader"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Stock Transfer Order updated successfully.",
                Data = result
            };
        }
    }
}
