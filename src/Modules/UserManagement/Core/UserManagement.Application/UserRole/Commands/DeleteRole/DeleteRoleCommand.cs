using UserManagement.Application.Common.HttpResponse;
using UserManagement.Application.UserRole.Queries.GetRole;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UserManagement.Application.UserRole.Commands.DeleteRole
{
    public class DeleteRoleCommand :IRequest<int>
    
    {
        public int Id { get; set; } 
                
    }
}