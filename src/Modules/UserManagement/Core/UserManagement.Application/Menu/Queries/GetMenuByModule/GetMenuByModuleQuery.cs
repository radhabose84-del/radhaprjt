using MediatR;

namespace UserManagement.Application.Menu.Queries.GetMenuByModule
{
    public class GetMenuByModuleQuery : IRequest<List<MenuDTO>>
    {
        public List<int>? ModuleId { get; set; }
    }
}