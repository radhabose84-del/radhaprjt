using Core.Application.Common.HttpResponse;
using Core.Application.UserRole.Queries.GetRole;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.UserRole.Commands.UpdateRole
{  
   
    public class UpdateRoleCommand : IRequest<bool>
    {
         public int Id { get; set; }
        public string? RoleName { get; set; }
        public string? Description { get; set; }
        public int CompanyId { get; set; }
        public byte  IsActive { get; set; }
       
    }
}