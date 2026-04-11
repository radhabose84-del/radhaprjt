using Contracts.Common;
using UserManagement.Application.Users.Commands.CreateUser;
using UserManagement.Application.Users.Queries.GetUsers;
using MediatR;

namespace UserManagement.Application.Users.Commands.UpdateUser
{
    public class UpdateUserCommand : IRequest<ApiResponseDTO<bool>>
    {
    public int UserId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? UserName { get; set; }
    public byte IsActive { get; set; }
    public string? Mobile { get; set; }
    public string? EmailId { get; set; }
    public int UserGroupId { get; set; }
    public int DepartmentId { get; set; }
    public int EntityId { get; set; }
    public int? EmpId { get; set; }
    public List<UserDivisionDTO>? userDivisions { get; set; }
    public List<UserCompanyDTO>? UserCompanies  { get; set; }
    public List<UserRoleAllocationDTO>? userRoleAllocations { get; set; }
    public List<UserUnitDTO>? userUnits { get; set; }
    public List<UserDepartmentDTO>? userDepartments { get; set; } 

    }
}