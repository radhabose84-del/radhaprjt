using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain.Entities;
using Core.Application.Common.Mappings;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Application.UserRole.Queries.GetRole
{

   public class UserRoleDto : IMapFrom<Core.Domain.Entities.UserRole>
    {
         public int UserRoleId  { get; set; }
        public string? RoleName { get; set; }
        public string? Description { get; set; }
        public int CompanyId { get; set; }
     
        
    }
}