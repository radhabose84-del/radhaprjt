using AutoMapper;
using UserManagement.Application.Common.Interfaces.IMenu;
using UserManagement.Domain.Events;
using MediatR;

namespace UserManagement.Application.Menu.Commands.UpdateMenu
{
    public class UpdateMenuCommandHandler : IRequestHandler<UpdateMenuCommand, bool>
    {
        private readonly IMenuCommand _menuCommand;
        private readonly IMapper _imapper;
        private readonly IMediator _mediator;
        public UpdateMenuCommandHandler(IMenuCommand menuCommand, IMapper imapper, IMediator mediator)
        {
            _menuCommand = menuCommand;
            _imapper = imapper;
            _mediator = mediator;
        }
        public async Task<bool> Handle(UpdateMenuCommand request, CancellationToken cancellationToken)
        {
            var Menu  = _imapper.Map<UserManagement.Domain.Entities.Menu>(request);
         
                var MenuResult = await _menuCommand.UpdateAsync(Menu);

                
                    var domainEvent = new AuditLogsDomainEvent(
                        actionDetail: "Update",
                        actionCode: "Update Menu",
                        actionName: "Update Menu",
                        details: $"Menu was updated.",
                        module:"Menu"
                    );               
                    await _mediator.Publish(domainEvent, cancellationToken); 
              
                if(MenuResult)
                {
                    return MenuResult;
                }
                throw new Exception("Menu not updated.");
                
        }
    }
}