using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesChannel;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesChannel.Commands.CreateSalesChannel
{
    public class CreateSalesChannelCommandHandler : IRequestHandler<CreateSalesChannelCommand, ApiResponseDTO<int>>
    {
        private readonly ISalesChannelCommandRepository _commandRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateSalesChannelCommandHandler(
            ISalesChannelCommandRepository commandRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateSalesChannelCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.SalesChannel>(request);

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "SALES_CHANNEL_CREATE",
                actionName: request.SalesChannelCode,
                details: $"Sales Channel '{request.SalesChannelCode}' created successfully with Id {newId}.",
                module: "SalesChannel"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Sales Channel created successfully.",
                Data = newId
            };
        }
    }
}
