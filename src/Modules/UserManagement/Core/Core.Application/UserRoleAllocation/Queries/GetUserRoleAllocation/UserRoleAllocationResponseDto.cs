using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain.Entities;
using Core.Application.Common.Mappings;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Application.UserRoleAllocation.Queries.GetUserRoleAllocation
{

   public class UserRoleAllocationResponseDto : IMapFrom<Core.Domain.Entities.UserRoleAllocation>
    {
    public int UserRoleAllocationId { get; set; }
    public int UserId { get; set; }
    public int UserRoleId { get; set; }
    public string? RoleName { get; set; }
        
    }
}