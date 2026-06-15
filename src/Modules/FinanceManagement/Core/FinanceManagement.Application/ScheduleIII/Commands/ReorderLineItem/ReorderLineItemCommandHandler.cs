using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Commands.ReorderLineItem
{
    public class ReorderLineItemCommandHandler : IRequestHandler<ReorderLineItemCommand, ApiResponseDTO<bool>>
    {
        private readonly IScheduleIIICommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public ReorderLineItemCommandHandler(
            IScheduleIIICommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<bool>> Handle(ReorderLineItemCommand request, CancellationToken cancellationToken)
        {
            var result = await _commandRepository.ReorderLineItemAsync(request.LineItemId, request.Direction, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Reorder",
                actionCode: "S3_LINEITEM_REORDER",
                actionName: request.LineItemId.ToString(),
                details: $"Schedule III line item {request.LineItemId} reordered (direction {request.Direction}).",
                module: "ScheduleIIILineItem"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<bool>
            {
                IsSuccess = result,
                Message = result ? "Line item reordered successfully." : "Line item could not be reordered.",
                Data = result
            };
        }
    }
}
