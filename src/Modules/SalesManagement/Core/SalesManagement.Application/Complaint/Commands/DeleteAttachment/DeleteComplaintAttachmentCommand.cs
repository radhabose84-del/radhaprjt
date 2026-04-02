using MediatR;

namespace SalesManagement.Application.Complaint.Commands.DeleteAttachment
{
    public sealed record DeleteComplaintAttachmentCommand(int Id) : IRequest<bool>;
}
