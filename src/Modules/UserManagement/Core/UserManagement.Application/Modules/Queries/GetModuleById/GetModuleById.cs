using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using UserManagement.Application.Modules.Queries.GetModules;
using UserManagement.Application.RoleEntitlements.Queries.GetRoleEntitlements;
using MediatR;

namespace UserManagement.Application.Modules.Queries.GetModuleById
{
    public class GetModuleByIdQuery: IRequest<ModuleByIdDto>
    {
        public int Id { get; set; }
        
    }
}