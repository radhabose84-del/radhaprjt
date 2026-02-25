namespace UserManagement.Domain.Entities
{
    public class RoleParent
    {
        public int Id { get; set; }
        public int RoleId { get; set; }
        public UserRole? Role { get; set; }
        public int MenuId { get; set; }
        public Menu? Menu { get; set; }
    }
}