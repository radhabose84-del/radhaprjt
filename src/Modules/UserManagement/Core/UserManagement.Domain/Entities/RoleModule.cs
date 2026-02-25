namespace UserManagement.Domain.Entities
{
    public class RoleModule
    {
        public int Id { get; set; }
        public int RoleId { get; set; }
        public UserRole? Role { get; set; }
        public int ModuleId { get; set; }
        public Modules? Module { get; set; }
        // public RoleMenu ChildMenu { get; set; } 
    }
}