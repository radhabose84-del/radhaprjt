using Contracts.Common;
using UserManagement.Application.Users.Queries.GetUsers;
using MediatR;

namespace UserManagement.Application.Users.Commands.CreateUser
{
    public class CreateUserCommand : IRequest<ApiResponseDTO<UserDto>>
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public string? Mobile { get; set; }
        public string? EmailId { get; set; }
        public int UserGroupId { get; set; }
        public int  DepartmentId { get; set; }
        public int EntityId { get; set; }
        public int? EmpId { get; set; }
        public int? ReportToId { get; set; }
        public List<UserDivisionDTO> userDivisions { get; set; } = new();
        public List<UserCompanyDTO> UserCompanies { get; set; } = new();
        public List<UserRoleAllocationDTO> userRoleAllocations { get; set; } = new();
        public List<UserUnitDTO> userUnits { get; set; } = new();
        public List<UserDepartmentDTO> userDepartments { get; set; } = new();
    }
}