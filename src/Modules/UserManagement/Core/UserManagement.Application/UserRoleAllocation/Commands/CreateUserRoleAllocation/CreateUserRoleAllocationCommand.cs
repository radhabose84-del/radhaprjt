using UserManagement.Application.UserRoleAllocation.Queries.GetUserRoleAllocation;
using MediatR;
using Contracts.Common;

namespace UserManagement.Application.UserRoleAllocation.Commands.CreateUserRoleAllocation
{
    public class CreateUserRoleAllocationCommand  : IRequest<List<UserRoleAllocationResponseDto>>, IRequirePermission
    {
        
    public CreateUserRoleAllocationDto UserRoleAllocationDto { get; set; }

    public CreateUserRoleAllocationCommand(CreateUserRoleAllocationDto dto)
    {
        UserRoleAllocationDto = dto;
    }
       
    public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
