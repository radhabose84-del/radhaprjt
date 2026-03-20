using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.Application.UserRole.Queries.GetRole
{
    public class GetUserRoleDto
    {
        public int Id { get; set; }
        public string? RoleName { get; set; }
        public string? Description { get; set; }
        public int CompanyId { get; set; }
        public Status IsActive { get; set; }
        public IsDelete IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string? ModifiedByName { get; set; }
        public string? ModifiedIP { get; set; }
        public List<RoleItemGroupMappingOutputDto>? RoleItemGroupMappings { get; set; }
    }

    public class RoleItemGroupMappingOutputDto
    {
        public int Id { get; set; }
        public int ItemGroupId { get; set; }
        public string? ItemGroupName { get; set; }
    }
}