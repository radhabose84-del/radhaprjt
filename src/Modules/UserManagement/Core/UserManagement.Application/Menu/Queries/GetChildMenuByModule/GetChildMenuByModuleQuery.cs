using MediatR;

namespace UserManagement.Application.Menu.Queries.GetChildMenuByModule
{
    public class GetChildMenuByModuleQuery : IRequest<List<ChildMenuDTO>>
    {
        public List<int>? ParentId { get; set; }
    }
}