using UserManagement.Application.UserRole.Queries.GetRole;
using MediatR;

namespace UserManagement.Application.UserRole.Queries.GetRoleById
{
    public class GetRoleByIdQuery :IRequest<GetUserRoleDto>
    {
      public int Id { get; set; }
    }
}