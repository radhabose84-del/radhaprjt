using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IGlAccountMaster;
using FinanceManagement.Application.GlAccountMaster.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.GlAccountMaster.Queries.GetGlAccountFavourites
{
    public class GetGlAccountFavouritesQueryHandler : IRequestHandler<GetGlAccountFavouritesQuery, IReadOnlyList<AccountSearchResultDto>>
    {
        private readonly IGlAccountMasterQueryRepository _queryRepository;
        private readonly IGlAccountUserPrefStore _prefStore;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;

        public GetGlAccountFavouritesQueryHandler(
            IGlAccountMasterQueryRepository queryRepository,
            IGlAccountUserPrefStore prefStore,
            IIPAddressService ipAddressService,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _prefStore = prefStore;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<AccountSearchResultDto>> Handle(GetGlAccountFavouritesQuery request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");
            var userId = _ipAddressService.GetUserId();

            var favouriteIds = await _prefStore.GetFavouriteAccountIdsAsync(userId, companyId, cancellationToken);

            var rows = favouriteIds.Count == 0
                ? new List<AccountSearchResultDto>()
                : (await _queryRepository.GetByIdsForSearchAsync(favouriteIds.ToList(), companyId, cancellationToken)).ToList();

            foreach (var r in rows)
                r.IsFavourite = true;

            var ranked = rows.OrderBy(r => r.AccountCode).ToList();

            await _mediator.Publish(new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetGlAccountFavouritesQuery",
                actionName: ranked.Count.ToString(),
                details: $"Account favourites fetched ({ranked.Count}).",
                module: "GlAccountMaster"), cancellationToken);

            return ranked;
        }
    }
}
