using MediatR;

namespace UserManagement.Application.UserRole.Commands.DeleteRole
{
    public class DeleteRoleCommand :IRequest<int>
    
    {
        public int Id { get; set; } 
                
    }
}