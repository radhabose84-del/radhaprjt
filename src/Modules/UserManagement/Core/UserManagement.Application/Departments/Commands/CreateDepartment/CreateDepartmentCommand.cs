using UserManagement.Application.Departments.Queries.GetDepartments;
using MediatR;

namespace UserManagement.Application.Departments.Commands.CreateDepartment
{

    public class CreateDepartmentCommand : IRequest<DepartmentDto>
    {

        public string? ShortName { get; set; }
        public string? DeptName { get; set; }
        public int CompanyId { get; set; }        
        public int DepartmentGroupId { get; set; }
         
    }
}