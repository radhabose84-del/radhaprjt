using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IComplaintDepartmentFeedback;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.ComplaintDepartmentFeedback.Commands.DeleteAttachment
{
    public class DeleteFeedbackAttachmentCommandHandler : IRequestHandler<DeleteFeedbackAttachmentCommand, bool>
    {
        private readonly IComplaintDepartmentFeedbackCommandRepository _commandRepository;
        private readonly IComplaintDepartmentFeedbackQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public DeleteFeedbackAttachmentCommandHandler(
            IComplaintDepartmentFeedbackCommandRepository commandRepository,
            IComplaintDepartmentFeedbackQueryRepository queryRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteFeedbackAttachmentCommand request, CancellationToken cancellationToken)
        {
            var filePath = await _queryRepository.GetAttachmentFilePathAsync(request.Id);
            if (filePath == null)
                throw new ExceptionRules("Attachment not found.");

            var result = await _commandRepository.DeleteAttachmentAsync(request.Id, cancellationToken);

            if (result && File.Exists(filePath))
                File.Delete(filePath);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Delete",
                actionCode: "FEEDBACK_ATTACHMENT_DELETE",
                actionName: request.Id.ToString(),
                details: $"Feedback attachment {request.Id} deleted.",
                module: "ComplaintDepartmentFeedback");
            await _mediator.Publish(auditEvent, cancellationToken);

            return result;
        }
    }
}
