using MediatR;
using Contracts.Common;

namespace UserManagement.Application.DepartmentGroup.Command.DeleteDepartmentGroup
{
    public class DeleteDepartmentGroupCommand  :IRequest<bool>, IRequirePermission 
    {
         public int Id { get; set; }
         public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
