using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Commands.ReorderDetail
{
    public class ReorderDetailCommandHandler : IRequestHandler<ReorderDetailCommand, ApiResponseDTO<bool>>
    {
        private readonly IScheduleIIICommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public ReorderDetailCommandHandler(IScheduleIIICommandRepository commandRepository, IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<bool>> Handle(ReorderDetailCommand request, CancellationToken cancellationToken)
        {
            var result = await _commandRepository.ReorderDetailAsync(request.Id, request.Direction, cancellationToken);

            if (!result)
                throw new ExceptionRules("Line could not be reordered (no neighbour in that direction).");

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Reorder",
                actionCode: "S3_DETAIL_REORDER",
                actionName: request.Id.ToString(),
                details: $"Schedule III line {request.Id} reordered ({(request.Direction == 1 ? "up" : "down")}).",
                module: "ScheduleIIIDetail"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<bool>
            {
                IsSuccess = true,
                Message = "Line reordered successfully.",
                Data = result
            };
        }
    }
}
