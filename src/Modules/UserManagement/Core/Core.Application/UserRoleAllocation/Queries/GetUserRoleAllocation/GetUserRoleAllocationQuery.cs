using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.UserRoleAllocation.Queries.GetUserRoleAllocation
{
    public class GetUserRoleAllocationQuery : IRequest<List<CreateUserRoleAllocationDto>>
    {
        
    }
}