using UserManagement.Domain.Common;

namespace UserManagement.Domain.Entities
{
    public class UserFavoriteMenu : BaseEntity
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int MenuId { get; set; }

        // Navigation property (same-module)
        public Menu? Menu { get; set; }
    }
}
