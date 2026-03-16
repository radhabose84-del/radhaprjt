using UserManagement.Domain.Common;

namespace UserManagement.Domain.Entities
{
    public class RoleItemGroupMapping : BaseEntity
    {
        public int Id { get; set; }
        public int RoleId { get; set; }
        public int ItemGroupId { get; set; }

        // Navigation (same-module FK)
        public UserRole? UserRole { get; set; }
    }
}
