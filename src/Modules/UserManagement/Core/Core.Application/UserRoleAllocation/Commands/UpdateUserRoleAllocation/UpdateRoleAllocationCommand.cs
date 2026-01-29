using Core.Application.UserRoleAllocation.Queries.GetUserRoleAllocation;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.UserRoleAllocation.Commands.UpdateUserRoleAllocation
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