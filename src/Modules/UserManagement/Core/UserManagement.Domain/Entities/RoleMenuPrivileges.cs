using UserManagement.Domain.Common;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.Domain.Entities
{
    public class RoleMenuPrivileges : BaseEntity
    {
        public RoleMenuPrivileges()
        {
            IsActive = Status.Active;
            IsDeleted = IsDelete.NotDeleted;
        }

        public int Id { get; set; }
        public int RoleId { get; set; }
        public int MenuId { get; set; }
        public bool CanView { get; set; }
        public bool CanAdd { get; set; }
        public bool CanUpdate { get; set; }
        public bool CanDelete { get; set; }
        public bool CanExport { get; set; }
        public bool CanApprove { get; set; }
        public UserRole? UserRole { get; set; }
        public Menu? Menu { get; set; }
    }
}
