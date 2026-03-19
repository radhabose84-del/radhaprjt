namespace UserManagement.Application.RoleItemGroupMapping.Dto
{
    public class RoleItemGroupMappingLookupDto
    {
        public int Id { get; set; }
        public int RoleId { get; set; }
        public string? RoleName { get; set; }
        public int ItemGroupId { get; set; }
        public string? ItemGroupName { get; set; }
    }
}
