using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IGlAccountMaster;
using FinanceManagement.Application.GlAccountMaster.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.GlAccountMaster.Queries.GetGlAccountSearch
{
    public class GetGlAccountSearchQueryHandler : IRequestHandler<GetGlAccountSearchQuery, IReadOnlyList<AccountSearchResultDto>>
    {
        private const int MaxTake = 50;

        private readonly IGlAccountMasterQueryRepository _queryRepository;
        private readonly IGlAccountUserPrefStore _prefStore;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;

        public GetGlAccountSearchQueryHandler(
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

        public async Task<IReadOnlyList<AccountSearchResultDto>> Handle(GetGlAccountSearchQuery request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");
            var userId = _ipAddressService.GetUserId();
            var take = Math.Clamp(request.Take <= 0 ? 20 : request.Take, 1, MaxTake);

            // Per-user shortcuts (favourites + recently-used) drive both ranking and the empty-term open state.
            var favouriteIds = (await _prefStore.GetFavouriteAccountIdsAsync(userId, companyId, cancellationToken)).ToHashSet();
            var recent = await _prefStore.GetRecentAsync(userId, companyId, take, cancellationToken);
            var recentRank = recent
                .Select((r, i) => (r.AccountId, i))
                .ToDictionary(x => x.AccountId, x => x.i);

            List<AccountSearchResultDto> rows;

            if (string.IsNullOrWhiteSpace(request.Term))
            {
                // Component just opened: show favourites + recently-used first. Fall back to a general
                // TOP-N list only when the user has no shortcuts yet.
                var shortcutIds = favouriteIds.Concat(recentRank.Keys).Distinct().ToList();
                rows = shortcutIds.Count > 0
                    ? (await _queryRepository.GetByIdsForSearchAsync(shortcutIds, companyId, cancellationToken)).ToList()
                    : (await _queryRepository.SearchAsync(null, companyId, request.AccountTypeId, request.AccountGroupId, request.ActiveOnly, take, cancellationToken)).ToList();
            }
            else
            {
                rows = (await _queryRepository.SearchAsync(
                    request.Term, companyId, request.AccountTypeId, request.AccountGroupId, request.ActiveOnly, take, cancellationToken)).ToList();
            }

            foreach (var r in rows)
            {
                r.IsFavourite = favouriteIds.Contains(r.Id);
                r.IsRecent = recentRank.ContainsKey(r.Id);
            }

            // Rank: favourites → recently-used (most recent first) → the rest (search/code order).
            var ranked = rows
                .Select((r, idx) => (r, idx))
                .OrderByDescending(x => x.r.IsFavourite)
                .ThenBy(x => recentRank.TryGetValue(x.r.Id, out var rank) ? rank : int.MaxValue)
                .ThenBy(x => x.idx)
                .Select(x => x.r)
                .ToList();

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetGlAccountSearchQuery",
                actionName: ranked.Count.ToString(),
                details: $"Account type-ahead search ('{request.Term}') returned {ranked.Count} rows.",
                module: "GlAccountMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return ranked;
        }
    }
}
