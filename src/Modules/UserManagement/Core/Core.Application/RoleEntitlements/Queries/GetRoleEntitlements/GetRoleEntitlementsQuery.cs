using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.RoleEntitlements.Queries.GetRoleEntitlements
{
    public class GetRoleEntitlementsQuery : IRequest<List<RoleEntitlementDto>>
    {
        public string RoleName { get; set; }
    }

}