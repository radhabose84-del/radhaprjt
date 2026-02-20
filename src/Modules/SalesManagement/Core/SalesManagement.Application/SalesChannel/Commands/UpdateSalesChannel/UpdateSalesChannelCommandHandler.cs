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
        private readonly ISalesChannelQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateSalesChannelCommandHandler(
            ISalesChannelCommandRepository commandRepository,
            ISalesChannelQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateSalesChannelCommand request, CancellationToken cancellationToken)
        {
            var existing = await _queryRepository.GetByIdAsync(request.Id);
            if (existing == null)
                throw new EntityNotFoundException($"Sales Channel with Id {request.Id} not found.");

            var entity = _mapper.Map<Domain.Entities.SalesChannel>(request);

            var updatedId = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "SALES_CHANNEL_UPDATE",
                actionName: existing.SalesChannelCode,
                details: $"Sales Channel '{existing.SalesChannelCode}' updated successfully.",
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
