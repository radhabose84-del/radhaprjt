using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.ComplaintDepartmentFeedback.Commands.DeleteAttachment
{
    public sealed record DeleteFeedbackAttachmentCommand(int Id) : IRequest<bool>, IRequirePermission
{
    public PermissionType RequiredPermission => PermissionType.CanDelete;
}
}
