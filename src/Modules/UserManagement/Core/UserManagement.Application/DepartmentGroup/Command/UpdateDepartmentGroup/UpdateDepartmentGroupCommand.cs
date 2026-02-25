using MediatR;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.Application.DepartmentGroup.Command.UpdateDepartmentGroup
{
    public class UpdateDepartmentGroupCommand  : IRequest<int>
    {
         public int Id { get; set; }
        public string? DepartmentGroupCode { get; set; }
        public string? DepartmentGroupName { get; set; }        
        public Status IsActive { get; set; }
    }
}