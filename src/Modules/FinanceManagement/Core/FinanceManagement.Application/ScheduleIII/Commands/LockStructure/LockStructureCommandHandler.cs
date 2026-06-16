using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Commands.LockStructure
{
    public class LockStructureCommandHandler : IRequestHandler<LockStructureCommand, ApiResponseDTO<bool>>
    {
        private readonly IScheduleIIICommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public LockStructureCommandHandler(
            IScheduleIIICommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<bool>> Handle(LockStructureCommand request, CancellationToken cancellationToken)
        {
            var result = await _commandRepository.LockStructureAsync(request.StructureId);

            if (!result)
                throw new ExceptionRules("Structure not found or 'Locked' status is not configured.");

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Lock",
                actionCode: "S3_STRUCTURE_LOCK",
                actionName: request.StructureId.ToString(),
                details: $"Schedule III structure {request.StructureId} locked.",
                module: "ScheduleIIIStructure"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<bool>
            {
                IsSuccess = true,
                Message = "Structure locked successfully.",
                Data = result
            };
        }
    }
}
