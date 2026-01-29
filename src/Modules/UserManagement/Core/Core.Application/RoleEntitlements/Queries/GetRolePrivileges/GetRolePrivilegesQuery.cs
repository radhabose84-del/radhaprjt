using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common.HttpResponse;
using Core.Application.RoleEntitlements.Commands.GetRolePrivileges;
using MediatR;

namespace Core.Application.RoleEntitlements.Queries.GetRolePrivileges
{
    public class GetRolePrivilegesQuery : IRequest<List<ModuleDTO>>
    {
        public int UserId { get; set; }
    }
}