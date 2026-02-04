using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Application.Common.HttpResponse;
using UserManagement.Application.RoleEntitlements.Queries.GetRoleEntitlements;
using MediatR;

namespace UserManagement.Application.RoleEntitlements.Commands.DeleteRoleEntitlement
{
    public class DeleteRoleEntitlementCommand :  IRequest<ApiResponseDTO<RoleEntitlementDto>> 
    {
        public int Id { get; set; }                
        
    }
}