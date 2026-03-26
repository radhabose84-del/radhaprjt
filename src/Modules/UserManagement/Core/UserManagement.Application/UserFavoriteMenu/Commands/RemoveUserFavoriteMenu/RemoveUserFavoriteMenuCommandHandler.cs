using Contracts.Interfaces;
using MediatR;
using UserManagement.Application.Common.Interfaces.IUserFavoriteMenu;
using UserManagement.Domain.Events;

namespace UserManagement.Application.UserFavoriteMenu.Commands.RemoveUserFavoriteMenu
{
    public class RemoveUserFavoriteMenuCommandHandler : IRequestHandler<RemoveUserFavoriteMenuCommand, bool>
    {
        private readonly IUserFavoriteMenuCommandRepository _commandRepository;
        private readonly IMediator _mediator;
        private readonly IIPAddressService _ipAddressService;

        public RemoveUserFavoriteMenuCommandHandler(
            IUserFavoriteMenuCommandRepository commandRepository,
            IMediator mediator,
            IIPAddressService ipAddressService)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
            _ipAddressService = ipAddressService;
        }

        public async Task<bool> Handle(RemoveUserFavoriteMenuCommand request, CancellationToken cancellationToken)
        {
            var userId = _ipAddressService.GetUserId();

            var result = await _commandRepository.HardDeleteAsync(userId, request.MenuId, cancellationToken);

            if (!result)
            {
                throw new Exception("Failed to remove menu from favorites.");
            }

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Delete",
                actionCode: "USERFAVORITEMENU_DELETE",
                actionName: request.MenuId.ToString(),
                details: $"Menu {request.MenuId} removed from favorites by user {userId}.",
                module: "UserFavoriteMenu"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
