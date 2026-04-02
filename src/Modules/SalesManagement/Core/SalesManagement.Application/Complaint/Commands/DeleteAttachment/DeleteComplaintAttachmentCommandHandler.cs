using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IComplaint;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.Complaint.Commands.DeleteAttachment
{
    public class DeleteComplaintAttachmentCommandHandler : IRequestHandler<DeleteComplaintAttachmentCommand, bool>
    {
        private readonly IComplaintCommandRepository _commandRepository;
        private readonly IComplaintQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public DeleteComplaintAttachmentCommandHandler(
            IComplaintCommandRepository commandRepository,
            IComplaintQueryRepository queryRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteComplaintAttachmentCommand request, CancellationToken cancellationToken)
        {
            var filePath = await _queryRepository.GetAttachmentFilePathAsync(request.Id);
            if (filePath == null)
                throw new ExceptionRules("Attachment not found.");

            var result = await _commandRepository.DeleteAttachmentAsync(request.Id, cancellationToken);

            if (result && File.Exists(filePath))
                File.Delete(filePath);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Delete",
                actionCode: "COMPLAINT_ATTACHMENT_DELETE",
                actionName: request.Id.ToString(),
                details: $"Complaint attachment {request.Id} deleted.",
                module: "Complaint");
            await _mediator.Publish(auditEvent, cancellationToken);

            return result;
        }
    }
}
