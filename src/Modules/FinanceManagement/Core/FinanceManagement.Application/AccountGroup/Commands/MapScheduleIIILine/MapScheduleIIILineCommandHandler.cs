using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.IAccountGroup;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.AccountGroup.Commands.MapScheduleIIILine
{
    public class MapScheduleIIILineCommandHandler : IRequestHandler<MapScheduleIIILineCommand, ApiResponseDTO<int>>
    {
        private readonly IAccountGroupCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public MapScheduleIIILineCommandHandler(
            IAccountGroupCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<int>> Handle(MapScheduleIIILineCommand request, CancellationToken cancellationToken)
        {
            var result = await _commandRepository.MapScheduleIIILineAsync(request.AccountGroupId, request.ScheduleIIILineItemId);

            var mapped = request.ScheduleIIILineItemId.HasValue;
            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: mapped ? "MapScheduleIIILine" : "UnmapScheduleIIILine",
                actionCode: "ACCOUNT_GROUP_SCHEDULE_III_MAP",
                actionName: request.AccountGroupId.ToString(),
                details: mapped
                    ? $"Account Group {request.AccountGroupId} mapped to Schedule III line {request.ScheduleIIILineItemId}."
                    : $"Account Group {request.AccountGroupId} Schedule III mapping removed.",
                module: "AccountGroup"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = mapped ? "Schedule III line mapped successfully." : "Schedule III mapping removed.",
                Data = result
            };
        }
    }
}
