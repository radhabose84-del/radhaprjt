using MediatR;

namespace UserManagement.Application.Menu.Commands.UpdateMenu
{
    public class UpdateMenuCommand : IRequest<bool>
    {
        public int Id { get; set; }
        public string MenuName { get; set; } = default!;
        public int ModuleId { get; set; }
        public string? MenuIcon { get; set; }
        public string MenuUrl { get; set; } = default!;
        public int ParentId { get; set; }
        public int SortOrder { get; set; }
        public string? Type { get; set; }
        public byte IsActive { get; set; }
    }
}