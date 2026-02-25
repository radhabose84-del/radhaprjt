using MediatR;


namespace UserManagement.Application.Departments.Commands.DeleteDepartment
{

    public class DeleteDepartmentCommand :IRequest<int>
    {
        public int Id { get; set; }
    }

   
}