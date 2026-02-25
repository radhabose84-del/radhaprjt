namespace UserManagement.Application.RoleEntitlements.Commands.GetRolePrivileges
{
    public class RoleMenuDTO
    {
        public int Id { get; set; }
        public string? MenuName { get; set; }
        public string? MenuUrl { get; set; }
        public string? Type { get; set; }
        public List<MenuPrivileageDTO>? MenuPrivileages { get; set; }
        public List<RoleMenuDTO>? ChildMenus { get; set; }
        
        
    }
}