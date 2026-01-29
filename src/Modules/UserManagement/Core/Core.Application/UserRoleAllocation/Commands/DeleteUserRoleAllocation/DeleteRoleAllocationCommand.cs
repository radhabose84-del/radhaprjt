using Core.Application.UserRole.Queries.GetRole;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.DeleteUserRoleAllocation.Commands.DeleteUserRoleAllocation
{
    public class DeleteRoleAllocationCommand : IRequest<int>
    {    
        public int RoleAllocationId { get; set; }

        public DeleteRoleAllocationCommand(int roleAllocationId)
        {
            RoleAllocationId = roleAllocationId;
        }
    }
}