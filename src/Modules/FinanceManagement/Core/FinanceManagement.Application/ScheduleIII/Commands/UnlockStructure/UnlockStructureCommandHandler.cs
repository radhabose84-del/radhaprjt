using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Commands.UnlockStructure
{
    public class UnlockStructureCommandHandler : IRequestHandler<UnlockStructureCommand, ApiResponseDTO<bool>>
    {
        private readonly IScheduleIIICommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public UnlockStructureCommandHandler(IScheduleIIICommandRepository commandRepository, IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<bool>> Handle(UnlockStructureCommand request, CancellationToken cancellationToken)
        {
            var result = await _commandRepository.UnlockStructureAsync(request.ScheduleIIIHeaderId);

            if (!result)
                throw new ExceptionRules("Structure not found, is not locked, or 'Draft' status is not configured.");

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Unlock",
                actionCode: "S3_STRUCTURE_UNLOCK",
                actionName: request.ScheduleIIIHeaderId.ToString(),
                details: $"Schedule III structure header {request.ScheduleIIIHeaderId} unlocked (reverted to Draft).",
                module: "ScheduleIIIHeader"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<bool>
            {
                IsSuccess = true,
                Message = "Structure unlocked successfully.",
                Data = result
            };
        }
    }
}
