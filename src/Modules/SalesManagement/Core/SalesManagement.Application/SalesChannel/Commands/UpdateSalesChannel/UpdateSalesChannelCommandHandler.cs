#nullable disable
using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesChannel;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesChannel.Commands.UpdateSalesChannel
{
    public class UpdateSalesChannelCommandHandler : IRequestHandler<UpdateSalesChannelCommand, ApiResponseDTO<int>>
    {
        private readonly ISalesChannelCommandRepository _commandRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateSalesChannelCommandHandler(
            ISalesChannelCommandRepository commandRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateSalesChannelCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.SalesChannel>(request);

            var updatedId = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "SALES_CHANNEL_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Sales Channel with Id {request.Id} updated successfully.",
                module: "SalesChannel"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Sales Channel updated successfully.",
                Data = updatedId
            };
        }
    }
}
