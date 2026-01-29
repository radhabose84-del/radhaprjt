using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Core.Application.Common.HttpResponse;
using Core.Application.Common.Interfaces.IMenu;
using Core.Domain.Events;
using MediatR;

namespace Core.Application.Menu.Commands.DeleteMenu
{
    public class DeleteMenuCommandHandler : IRequestHandler<DeleteMenuCommand, bool>
    {
        private readonly IMenuCommand _menuCommand;
        private readonly IMapper _imapper;
        private readonly IMediator _mediator;
        public DeleteMenuCommandHandler(IMenuCommand menuCommand, IMapper imapper, IMediator mediator)
        {
            _menuCommand = menuCommand;
            _imapper = imapper;
            _mediator = mediator;
        }
        public async Task<bool> Handle(DeleteMenuCommand request, CancellationToken cancellationToken)
        {
             var Menu  = _imapper.Map<Core.Domain.Entities.Menu>(request);
            var MenuResult = await _menuCommand.DeleteAsync(request.Id, Menu);


                  //Domain Event  
                    var domainEvent = new AuditLogsDomainEvent(
                        actionDetail: "Delete",
                        actionCode: "Delete Menu",
                        actionName: "Delete Menu",
                        details: $"Menu was deleted.",
                        module:"Menu"
                    );               
                    await _mediator.Publish(domainEvent, cancellationToken);  

                 if(MenuResult)
                {
                    return MenuResult;
                }
            throw new Exception("Menu not deleted.");
        }
    }
}