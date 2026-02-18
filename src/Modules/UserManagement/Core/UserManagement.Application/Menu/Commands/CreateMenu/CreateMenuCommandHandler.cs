using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Common;
using UserManagement.Application.Common.Interfaces.IMenu;
using UserManagement.Domain.Events;
using MediatR;

namespace UserManagement.Application.Menu.Commands.CreateMenu
{
    public class CreateMenuCommandHandler : IRequestHandler<CreateMenuCommand, int>
    {
          private readonly IMenuCommand _menuCommand;
        private readonly IMapper _imapper;
        private readonly IMediator _mediator;
        public CreateMenuCommandHandler(IMenuCommand menuCommand, IMapper imapper, IMediator mediator)
        {
            _menuCommand = menuCommand;
            _imapper = imapper;
            _mediator = mediator;
        }
        public async Task<int> Handle(CreateMenuCommand request, CancellationToken cancellationToken)
        {
               
                 var Menu  = _imapper.Map<UserManagement.Domain.Entities.Menu>(request);

                var MenuResult = await _menuCommand.CreateAsync(Menu);
                
                
                if (MenuResult > 0)
                {
                    var domainEvent = new AuditLogsDomainEvent(
                     actionDetail: "Create",
                     actionCode: "Create Menu",
                     actionName: "Create Menu",
                     details: $"Menu was created. ",
                     module:"Menu"
                 );
                 await _mediator.Publish(domainEvent, cancellationToken);
                 
                    return MenuResult;
                }
               throw new Exception("Menu not created");
        }
    }
}