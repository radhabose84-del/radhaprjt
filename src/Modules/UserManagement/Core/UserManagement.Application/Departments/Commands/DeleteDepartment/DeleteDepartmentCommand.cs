using MediatR;
using Contracts.Common;


namespace UserManagement.Application.Departments.Commands.DeleteDepartment
{

    public class DeleteDepartmentCommand :IRequest<int>, IRequirePermission
    {
        public int Id { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }

   
}
