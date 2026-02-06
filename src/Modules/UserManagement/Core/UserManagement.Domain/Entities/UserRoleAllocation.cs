using UserManagement.Domain.Common;

namespace UserManagement.Domain.Entities
{
    public class UserRoleAllocation 
    {
    public int Id { get; set; }
    public int UserRoleId { get; set; } // Foreign Key
     public UserRole? UserRole { get; set; }
    public int UserId { get; set; }     // Foreign Key
    public User? User { get; set; }
    public byte IsActive { get; set; }
    }
}