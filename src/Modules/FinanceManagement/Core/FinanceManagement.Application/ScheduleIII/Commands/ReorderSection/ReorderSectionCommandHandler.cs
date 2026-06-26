using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Commands.ReorderSection
{
    public class ReorderSectionCommandHandler : IRequestHandler<ReorderSectionCommand, ApiResponseDTO<bool>>
    {
        private readonly IScheduleIIICommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public ReorderSectionCommandHandler(IScheduleIIICommandRepository commandRepository, IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<bool>> Handle(ReorderSectionCommand request, CancellationToken cancellationToken)
        {
            var result = await _commandRepository.ReorderSectionAsync(request.Id, request.Direction, cancellationToken);

            if (!result)
                throw new ExceptionRules("Section could not be reordered (no neighbour in that direction).");

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Reorder",
                actionCode: "S3_SECTION_REORDER",
                actionName: request.Id.ToString(),
                details: $"Schedule III section {request.Id} reordered ({(request.Direction == 1 ? "up" : "down")}).",
                module: "ScheduleIIISection"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<bool>
            {
                IsSuccess = true,
                Message = "Section reordered successfully.",
                Data = result
            };
        }
    }
}
