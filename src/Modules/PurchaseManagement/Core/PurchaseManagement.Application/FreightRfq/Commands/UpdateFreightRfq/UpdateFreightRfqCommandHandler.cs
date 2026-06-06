using AutoMapper;
using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IFreightRfq;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.FreightRfq.Commands.UpdateFreightRfq
{
    public class UpdateFreightRfqCommandHandler : IRequestHandler<UpdateFreightRfqCommand, ApiResponseDTO<int>>
    {
        private readonly IFreightRfqCommandRepository _commandRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public UpdateFreightRfqCommandHandler(
            IFreightRfqCommandRepository commandRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateFreightRfqCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<PurchaseManagement.Domain.Entities.FreightRfq.FreightRfqHeader>(request);

            var result = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "FREIGHTRFQ_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Freight RFQ with Id {request.Id} updated successfully.",
                module: "FreightRfq"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Freight RFQ updated successfully.",
                Data = result
            };
        }
    }
}
