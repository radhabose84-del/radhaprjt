using UserManagement.Application.Common.Mappings;
using UserManagement.Domain.Entities;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.Application.RoleItemGroupMapping.Dto
{
    public class RoleItemGroupMappingDto : IMapFrom<Domain.Entities.RoleItemGroupMapping>
    {
        public int Id { get; set; }
        public int RoleId { get; set; }
        public string? RoleName { get; set; }
        public int ItemGroupId { get; set; }
        public string? ItemGroupName { get; set; }
        public Status IsActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string? ModifiedByName { get; set; }
        public string? ModifiedIP { get; set; }
    }
}
