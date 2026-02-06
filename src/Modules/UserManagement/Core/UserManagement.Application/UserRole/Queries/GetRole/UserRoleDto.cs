using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Domain.Entities;
using UserManagement.Application.Common.Mappings;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserManagement.Application.UserRole.Queries.GetRole
{

   public class UserRoleDto : IMapFrom<UserManagement.Domain.Entities.UserRole>
    {
         public int UserRoleId  { get; set; }
        public string? RoleName { get; set; }
        public string? Description { get; set; }
        public int CompanyId { get; set; }
     
        
    }
}