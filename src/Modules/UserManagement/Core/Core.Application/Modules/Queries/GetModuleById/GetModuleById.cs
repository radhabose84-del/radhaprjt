using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common.HttpResponse;
using Core.Application.Modules.Queries.GetModules;
using Core.Application.RoleEntitlements.Queries.GetRoleEntitlements;
using MediatR;

namespace Core.Application.Modules.Queries.GetModuleById
{
    public class GetModuleByIdQuery: IRequest<ModuleByIdDto>
    {
        public int Id { get; set; }
        
    }
}