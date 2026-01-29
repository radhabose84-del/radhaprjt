using Core.Application.Common.HttpResponse;
using Core.Application.UserRole.Queries.GetRole;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.UserRole.Commands.CreateRole
{
    public class CreateRoleCommand  : IRequest<UserRoleDto>
    {         
        public string? RoleName { get; set; }
        public string? Description { get; set; }
        public int CompanyId { get; set; }
       
       
    }
}