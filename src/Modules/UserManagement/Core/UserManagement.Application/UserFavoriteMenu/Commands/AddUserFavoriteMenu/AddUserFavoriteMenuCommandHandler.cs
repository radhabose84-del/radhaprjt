using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using MediatR;
using UserManagement.Application.Common.Interfaces.IUserFavoriteMenu;
using UserManagement.Domain.Events;

namespace UserManagement.Application.UserFavoriteMenu.Commands.AddUserFavoriteMenu
{
    public class AddUserFavoriteMenuCommandHandler : IRequestHandler<AddUserFavoriteMenuCommand, ApiResponseDTO<int>>
    {
        private readonly IUserFavoriteMenuCommandRepository _commandRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IIPAddressService _ipAddressService;

        public AddUserFavoriteMenuCommandHandler(
            IUserFavoriteMenuCommandRepository commandRepository,
            IMediator mediator,
            IMapper mapper,
            IIPAddressService ipAddressService)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
            _mapper = mapper;
            _ipAddressService = ipAddressService;
        }

        public async Task<ApiResponseDTO<int>> Handle(AddUserFavoriteMenuCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.UserFavoriteMenu>(request);
            entity.UserId = _ipAddressService.GetUserId();

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "USERFAVORITEMENU_CREATE",
                actionName: request.MenuId.ToString(),
                details: $"Menu {request.MenuId} added to favorites by user {entity.UserId}.",
                module: "UserFavoriteMenu"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Menu added to favorites.",
                Data = newId
            };
        }
    }
}
