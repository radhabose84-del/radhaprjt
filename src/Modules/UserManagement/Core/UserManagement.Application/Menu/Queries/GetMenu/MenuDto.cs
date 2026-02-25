namespace UserManagement.Application.Menu.Queries.GetMenu
{
    public class MenuDto
    {
        public int Id { get; set; }
        public int ModuleId { get; set; }
        public string ModuleName { get; set; } = default!;
        public string MenuName { get; set; } = default!;
        public string MenuIcon { get; set; } = default!;
        public string MenuUrl { get; set; } = default!;
        public int ParentId { get; set; }
        public string ParentName { get; set; } = default!;
        public int SortOrder { get; set; }
        public string CreatedAt { get; set; } = default!;
        public string? Type { get; set; }
    }
}