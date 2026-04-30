using UserManagement.Application.Common.Mappings;
using UserManagement.Application.Users.Commands.CreateUser;
using UserManagement.Domain.Entities;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.Application.Users.Queries.GetUsers
{
    public class UserByIdDTO : IMapFrom<User>
    {
        public Guid Id { get; set; }
        public int UserId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? UserName { get; set; }
        public Status IsActive { get; set; }
        // public string? PasswordHash { get; set; }
        public int UserType { get; set; }
        public string? Mobile { get; set; }
        public string? EmailId { get; set; }
        public int EntityId { get; set; }
        public string? EntityName { get; set; }
        public int? EmpId { get; set; }
        public string? EmpName { get; set; }
        public int? ReportToId { get; set; }
        public List<UserDivisionDTO>? userDivisions { get; set; }
        public FirstTimeUserStatus IsFirstTimeUser { get; set; }
        public IsDelete IsDeleted { get; set; }
        public List<UserCompanyDTO>? UserCompanies { get; set; }
        public List<UserRoleAllocationDTO>? userRoleAllocations { get; set; }
        public List<UserUnitDTO>? UserUnits { get; set; }
        public int? UserGroupId { get; set; }
        public string? UserGroupName { get; set; }
        public int DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public List<UserDepartmentDTO>? userDepartments { get; set; }
    }
}