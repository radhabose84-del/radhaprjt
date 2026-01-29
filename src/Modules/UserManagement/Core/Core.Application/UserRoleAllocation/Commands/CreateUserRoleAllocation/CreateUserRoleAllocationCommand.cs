using Core.Application.UserRoleAllocation.Queries.GetUserRoleAllocation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.UserRoleAllocation.Commands.CreateUserRoleAllocation
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