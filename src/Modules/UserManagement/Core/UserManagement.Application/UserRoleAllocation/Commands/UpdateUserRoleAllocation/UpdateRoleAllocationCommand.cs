using UserManagement.Application.UserRoleAllocation.Queries.GetUserRoleAllocation;
using MediatR;

namespace UserManagement.Application.UserRoleAllocation.Commands.UpdateUserRoleAllocation
{

    public class UpdateRoleAllocationCommand : IRequest<List<UserRoleAllocationResponseDto>>
    {    
    public int UserId { get; set; }
    public List<int> NewRoleIds { get; set; }

    public UpdateRoleAllocationCommand(int userId, List<int> newRoleIds)
    {
        UserId = userId;
        NewRoleIds = newRoleIds;
    }
       
    }
}