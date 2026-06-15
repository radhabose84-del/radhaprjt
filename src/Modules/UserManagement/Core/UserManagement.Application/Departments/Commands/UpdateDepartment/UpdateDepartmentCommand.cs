using MediatR;
using static UserManagement.Domain.Enums.Common.Enums;
using Contracts.Common;

namespace UserManagement.Application.Departments.Commands.UpdateDepartment
{

    public class UpdateDepartmentCommand : IRequest<bool>, IRequirePermission
    {
        public int Id { get; set; }       
        public string? ShortName { get; set; }
        public string? DeptName { get; set; }
        public int CompanyId { get; set; }
        public int DepartmentGroupId { get; set; }
        public Status IsActive { get; set; }
             
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
