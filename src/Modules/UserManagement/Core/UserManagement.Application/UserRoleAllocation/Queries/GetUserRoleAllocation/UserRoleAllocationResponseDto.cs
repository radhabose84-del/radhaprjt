using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Domain.Entities;
using UserManagement.Application.Common.Mappings;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserManagement.Application.UserRoleAllocation.Queries.GetUserRoleAllocation
{

   public class UserRoleAllocationResponseDto : IMapFrom<UserManagement.Domain.Entities.UserRoleAllocation>
    {
    public int UserRoleAllocationId { get; set; }
    public int UserId { get; set; }
    public int UserRoleId { get; set; }
    public string? RoleName { get; set; }
        
    }
}