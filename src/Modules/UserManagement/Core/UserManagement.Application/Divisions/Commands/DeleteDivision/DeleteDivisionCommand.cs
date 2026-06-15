using MediatR;
using Contracts.Common;

namespace UserManagement.Application.Divisions.Commands.DeleteDivision
{
    public class DeleteDivisionCommand : IRequest<bool>, IRequirePermission
    {
        public int Id { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
