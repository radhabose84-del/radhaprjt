using MediatR;

namespace SalesManagement.Application.ComplaintDepartmentFeedback.Commands.DeleteAttachment
{
    public sealed record DeleteFeedbackAttachmentCommand(int Id) : IRequest<bool>;
}
