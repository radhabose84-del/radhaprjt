
using Contracts.Common;
using UserManagement.Application.UserRole.Queries.GetRole;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManagement.Application.UserRole.Queries.GetRoleById
{
    public class GetRoleByIdQuery :IRequest<GetUserRoleDto>
    {
      public int Id { get; set; }
    }
}