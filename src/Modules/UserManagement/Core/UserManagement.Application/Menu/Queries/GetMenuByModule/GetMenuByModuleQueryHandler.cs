#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Common;
using UserManagement.Application.Common.Interfaces.IMenu;
using UserManagement.Domain.Events;
using MediatR;

namespace UserManagement.Application.Menu.Queries.GetMenuByModule
{
    public class GetMenuByModuleQueryHandler : IRequestHandler<GetMenuByModuleQuery, List<MenuDTO>>
    {
        private readonly IMenuQuery _menuQuery;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        public GetMenuByModuleQueryHandler(IMenuQuery menuQuery, IMapper mapper, IMediator mediator)
        {
            _menuQuery = menuQuery;
            _mapper = mapper;
            _mediator = mediator;
        }
        public async Task<List<MenuDTO>> Handle(GetMenuByModuleQuery request, CancellationToken cancellationToken)
        {
              var menus = await _menuQuery.GetParentMenus(request.ModuleId);
            var menusList = _mapper.Map<List<MenuDTO>>(menus);

             //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetMenuByModule",
                    actionCode: "",        
                    actionName: "",
                    details: $"Menu details was fetched.",
                    module:"Menu"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
            return menusList;
        }
    }
}