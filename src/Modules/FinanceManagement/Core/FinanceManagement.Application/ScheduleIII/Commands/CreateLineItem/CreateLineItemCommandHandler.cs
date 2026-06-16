using AutoMapper;
using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Commands.CreateLineItem
{
    public class CreateLineItemCommandHandler : IRequestHandler<CreateLineItemCommand, ApiResponseDTO<int>>
    {
        private readonly IScheduleIIICommandRepository _commandRepository;
        private readonly IScheduleIIIQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateLineItemCommandHandler(
            IScheduleIIICommandRepository commandRepository,
            IScheduleIIIQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateLineItemCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.ScheduleIIILineItem>(request);

            var newId = await _commandRepository.CreateLineItemAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "S3_LINEITEM_CREATE",
                actionName: request.LineName ?? string.Empty,
                details: $"Schedule III line item '{request.LineName}' created successfully with Id {newId}.",
                module: "ScheduleIIILineItem"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Line item created successfully.",
                Data = newId
            };
        }
    }
}
