using MediatR;

namespace UserManagement.Application.DepartmentGroup.Command.DeleteDepartmentGroup
{
    public class DeleteDepartmentGroupCommand  :IRequest<bool> 
    {
         public int Id { get; set; }
    }
}