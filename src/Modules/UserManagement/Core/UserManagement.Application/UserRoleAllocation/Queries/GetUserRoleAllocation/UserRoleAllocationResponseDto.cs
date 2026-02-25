using UserManagement.Application.Common.Mappings;

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