using UserManagement.Application.Common.Mappings;

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