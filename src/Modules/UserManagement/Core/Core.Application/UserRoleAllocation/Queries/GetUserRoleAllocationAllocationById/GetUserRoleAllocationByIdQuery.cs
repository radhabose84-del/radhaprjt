
using Core.Application.UserRoleAllocation.Queries.GetUserRoleAllocation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.UserRoleAllocation.Queries.GetUserRoleAllocationById
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