using UserManagement.Application.Departments.Queries.GetDepartments;
using MediatR;
using Contracts.Common;

namespace UserManagement.Application.Departments.Commands.CreateDepartment
{

    public class CreateDepartmentCommand : IRequest<DepartmentDto>, IRequirePermission
    {

        public string? ShortName { get; set; }
        public string? DeptName { get; set; }
        public int CompanyId { get; set; }        
        public int DepartmentGroupId { get; set; }
         
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
