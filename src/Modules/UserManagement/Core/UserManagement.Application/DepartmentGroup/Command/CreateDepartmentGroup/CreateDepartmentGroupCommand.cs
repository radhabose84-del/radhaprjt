using MediatR;
using Contracts.Common;

namespace UserManagement.Application.DepartmentGroup.Command.CreateDepartmentGroup
{
    public class CreateDepartmentGroupCommand : IRequest<int>, IRequirePermission
    {   
        public string? DepartmentGroupCode { get; set; }
        public string? DepartmentGroupName { get; set; }       
        public byte IsActive { get; set; }
  
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
