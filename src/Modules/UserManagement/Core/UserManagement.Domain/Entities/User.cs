using UserManagement.Domain.Common;
using static UserManagement.Domain.Enums.Common.Enums;
using System.ComponentModel.DataAnnotations;

namespace UserManagement.Domain.Entities
{
    public class User : BaseEntity
    {
    public Guid Id { get; set; }
    [Key]
    public int UserId { get; set; }// Identity column
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? UserName { get; set; }
    public FirstTimeUserStatus IsFirstTimeUser { get; set; } 
    public string? PasswordHash { get; set; }
    public int? UserType { get; set; }
    public string? Mobile { get; set; }
    public string? EmailId { get; set; } 
    public byte IsLocked  { get; set; }  
    public int? PartyId { get; set; }     
    public int DepartmentId { get; set; }
    public Department Department { get; set; } = null!;
    public IList<UserRoleAllocation>? UserRoleAllocations { get; set; }

     public ICollection<PasswordLog>? Passwords { get; set; }
     public IList<UserCompany>? UserCompanies { get; set; }
    
     public IList<UserUnit>? UserUnits { get; set; }
     public int? EntityId { get; set; }
     public Entity? Entity { get; set; }
     public int? UserGroupId { get; set; }
     public UserGroup? UserGroup { get; set; }
     public IList<UserDivision>? UserDivisions { get; set; }
     public IList<UserDepartment>? UserDepartments { get; set; }

    public void SetPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be null or empty", nameof(password));

        // Generate a valid BCrypt hash
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
    }

    }
}