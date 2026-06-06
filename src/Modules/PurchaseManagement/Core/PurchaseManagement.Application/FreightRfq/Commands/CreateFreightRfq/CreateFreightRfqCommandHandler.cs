using AutoMapper;
using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IFreightRfq;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.FreightRfq.Commands.CreateFreightRfq
{
    public class CreateFreightRfqCommandHandler : IRequestHandler<CreateFreightRfqCommand, ApiResponseDTO<int>>
    {
        private readonly IFreightRfqCommandRepository _commandRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public CreateFreightRfqCommandHandler(
            IFreightRfqCommandRepository commandRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateFreightRfqCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<PurchaseManagement.Domain.Entities.FreightRfq.FreightRfqHeader>(request);

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "FREIGHTRFQ_CREATE",
                actionName: entity.FreightRfqNumber ?? newId.ToString(),
                details: $"Freight RFQ '{entity.FreightRfqNumber}' created successfully with Id {newId}.",
                module: "FreightRfq"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Freight RFQ created successfully.",
                Data = newId
            };
        }
    }
}
