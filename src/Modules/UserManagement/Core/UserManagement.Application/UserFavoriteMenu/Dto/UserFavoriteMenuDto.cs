namespace UserManagement.Application.UserFavoriteMenu.Dto
{
    public class UserFavoriteMenuDto
    {
        public int MenuId { get; set; }
        public string? MenuName { get; set; }
        public string? MenuUrl { get; set; }
        public string? MenuIcon { get; set; }
        public int ModuleId { get; set; }
        public string? ModuleName { get; set; }
    }
}
