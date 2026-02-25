using UserManagement.Domain.Common;

namespace UserManagement.Domain.Entities
{
    public class Menu : BaseEntity
    {
    public int Id { get; set; }
    public string? MenuName { get; set; }
    public int ModuleId { get; set; }
    public int ParentId { get; set; }
    public Menu? Parent { get; set; }
    public List<Menu> ChildMenus { get; set; } = new();
    public string? MenuUrl { get; set; }
    public string? MenuIcon { get; set; }
    public int SortOrder  { get; set; }
    public string? Type { get; set; }
    public Modules? Module { get; set; }
    public IList<RoleMenuPrivileges>? RoleMenus { get; set; }
    public IList<RoleParent>? RoleParents { get; set; }
    public IList<RoleChild>? RoleChildren { get; set; }
    public IList<CustomFieldMenu>? CustomFieldMenus { get; set; }


    }
}