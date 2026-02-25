
using UserManagement.Application.UserRoleAllocation.Queries.GetUserRoleAllocation;
using MediatR;

namespace UserManagement.Application.UserRoleAllocation.Queries.GetUserRoleAllocationById
{
    public class GetUserRoleAllocationByIdQuery :IRequest<CreateUserRoleAllocationDto>
    {
      public int UserId  { get; set; }
        public GetUserRoleAllocationByIdQuery(int userId)
        {
            UserId = userId;
        }
    }
}