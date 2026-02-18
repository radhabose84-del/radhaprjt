using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using UserManagement.Application.RoleEntitlements.Commands.GetRolePrivileges;
using MediatR;

namespace UserManagement.Application.RoleEntitlements.Queries.GetRolePrivileges
{
    public class GetRolePrivilegesQuery : IRequest<List<ModuleDTO>>
    {
        public int UserId { get; set; }
    }
}