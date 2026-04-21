using MediatR;

namespace UserManagement.Application.Menu.Queries.GetParentMenu
{
    public class GetParentMenuQuery : IRequest<List<ParentMenuDto>>
    {
        public string? SearchPattern { get; set; }
        public int? ModuleId { get; set; }
    }
}