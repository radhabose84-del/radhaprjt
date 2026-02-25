#nullable disable
using AutoMapper;
using UserManagement.Application.Common.Interfaces.IMenu;
using UserManagement.Domain.Events;
using MediatR;

namespace UserManagement.Application.Menu.Queries.GetChildMenuByModule
{
    public class GetChildMenuByModuleQueryHandler : IRequestHandler<GetChildMenuByModuleQuery, List<ChildMenuDTO>>
    {
          private readonly IMenuQuery _menuQuery;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        public GetChildMenuByModuleQueryHandler(IMenuQuery menuQuery, IMapper mapper, IMediator mediator)
        {
            _menuQuery = menuQuery;
            _mapper = mapper;
            _mediator = mediator;   
        }
        public async Task<List<ChildMenuDTO>> Handle(GetChildMenuByModuleQuery request, CancellationToken cancellationToken)
        {
             var menus = await _menuQuery.GetChildMenus(request.ParentId);
            var menusList = _mapper.Map<List<ChildMenuDTO>>(menus);

             //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetChildMenuByParent",
                    actionCode: "",        
                    actionName: "",
                    details: $"Child Menu details was fetched.",
                    module:"Child Menu"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
            return  menusList;
        }
    }
}