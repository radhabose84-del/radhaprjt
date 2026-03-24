using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IComplaint;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.Complaint.Commands.DeleteComplaint
{
    public class DeleteComplaintCommandHandler : IRequestHandler<DeleteComplaintCommand, bool>
    {
        private readonly IComplaintCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteComplaintCommandHandler(
            IComplaintCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteComplaintCommand request, CancellationToken cancellationToken)
        {
            var result = await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            if (!result)
                throw new ExceptionRules("Complaint not found.");

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "COMPLAINT_DELETE",
                actionName: request.Id.ToString(),
                details: $"Complaint with Id {request.Id} soft deleted.",
                module: "Complaint");
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
