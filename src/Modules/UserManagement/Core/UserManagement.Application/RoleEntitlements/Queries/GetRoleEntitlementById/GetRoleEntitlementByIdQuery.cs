using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UserManagement.Application.Common.HttpResponse;
using UserManagement.Application.RoleEntitlements.Queries.GetRoleEntitlements;
using MediatR;

namespace UserManagement.Application.RoleEntitlements.Queries.GetRoleEntitlementById
{
    public class GetRoleEntitlementByIdQuery: IRequest<GetByIdRoleEntitlementDTO>
    {
        public int Id { get; set; }
        
        
    }
}