using AutoMapper;
using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Commands.UpdateLineItem
{
    public class UpdateLineItemCommandHandler : IRequestHandler<UpdateLineItemCommand, ApiResponseDTO<int>>
    {
        private readonly IScheduleIIICommandRepository _commandRepository;
        private readonly IScheduleIIIQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateLineItemCommandHandler(
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

        public async Task<ApiResponseDTO<int>> Handle(UpdateLineItemCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.ScheduleIIILineItem>(request);

            var result = await _commandRepository.UpdateLineItemAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "S3_LINEITEM_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Schedule III line item with Id {request.Id} updated successfully.",
                module: "ScheduleIIILineItem"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Line item updated successfully.",
                Data = result
            };
        }
    }
}
