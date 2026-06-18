using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Commands.DeleteMaster
{
    public class DeleteMasterCommandHandler : IRequestHandler<DeleteMasterCommand, bool>
    {
        private readonly IScheduleIIICommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteMasterCommandHandler(IScheduleIIICommandRepository commandRepository, IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteMasterCommand request, CancellationToken cancellationToken)
        {
            var deleted = await _commandRepository.SoftDeleteDetailAsync(request.Id, cancellationToken);
            if (!deleted)
                throw new ExceptionRules("Schedule III line not found.");

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "S3_DETAIL_DELETE",
                actionName: request.Id.ToString(),
                details: $"Schedule III line with Id {request.Id} removed successfully.",
                module: "ScheduleIIIDetail"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
