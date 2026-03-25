using Contracts.Interfaces;
using MediatR;
using UserManagement.Application.Common.Interfaces.IUserFavoriteMenu;
using UserManagement.Application.UserFavoriteMenu.Dto;
using UserManagement.Domain.Events;

namespace UserManagement.Application.UserFavoriteMenu.Queries.GetUserFavoriteMenus
{
    public class GetUserFavoriteMenusQueryHandler : IRequestHandler<GetUserFavoriteMenusQuery, List<UserFavoriteMenuDto>>
    {
        private readonly IUserFavoriteMenuQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IIPAddressService _ipAddressService;

        public GetUserFavoriteMenusQueryHandler(
            IUserFavoriteMenuQueryRepository queryRepository,
            IMediator mediator,
            IIPAddressService ipAddressService)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
            _ipAddressService = ipAddressService;
        }

        public async Task<List<UserFavoriteMenuDto>> Handle(GetUserFavoriteMenusQuery request, CancellationToken cancellationToken)
        {
            var userId = _ipAddressService.GetUserId();
            var data = await _queryRepository.GetByUserIdAsync(userId);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetUserFavoriteMenus",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "User favorite menus were fetched.",
                module: "UserFavoriteMenu"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return data;
        }
    }
}
