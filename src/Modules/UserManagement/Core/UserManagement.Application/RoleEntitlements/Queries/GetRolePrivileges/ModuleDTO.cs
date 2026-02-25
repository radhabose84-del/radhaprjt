namespace UserManagement.Application.RoleEntitlements.Commands.GetRolePrivileges
{
    public class ModuleDTO
    {
        public int Id { get; set; }
        public string? ModuleName { get; set; }
        public List<RoleMenuDTO>? Menus { get; set; }
    }
}