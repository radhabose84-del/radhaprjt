using UserManagement.Application.UserRoleAllocation.Queries.GetUserRoleAllocation;
using MediatR;

namespace UserManagement.Application.UserRoleAllocation.Commands.CreateUserRoleAllocation
{
    public class CreateUserRoleAllocationCommand  : IRequest<List<UserRoleAllocationResponseDto>>
    {
        
    public CreateUserRoleAllocationDto UserRoleAllocationDto { get; set; }

    public CreateUserRoleAllocationCommand(CreateUserRoleAllocationDto dto)
    {
        UserRoleAllocationDto = dto;
    }
       
    }
}