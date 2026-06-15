using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.Complaint.Commands.DeleteComplaint
{
    public sealed record DeleteComplaintCommand(int Id) : IRequest<bool>, IRequirePermission
{
    public PermissionType RequiredPermission => PermissionType.CanDelete;
}
}
