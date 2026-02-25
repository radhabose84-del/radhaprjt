using UserManagement.Application.Departments.Queries.GetDepartments;
using MediatR;


namespace UserManagement.Application.Departments.Queries.GetDepartmentById
{

    public class GetDepartmentByIdQuery :IRequest<GetDepartmentDto>
    {
        
        public int DepartmentId { get; set; }
        
    }
}