using InventoryManagement.Application.Common.Interfaces.Item.ItemDetail.Queries;
using InventoryManagement.Application.Item.ItemDetail.Queries.GetItemAutoComplete;
using MediatR;

namespace InventoryManagement.Application.Item.ItemDetail.Queries.GetItemsByIds
{
    public sealed class GetItemsMasterByIdsQueryHandler 
        : IRequestHandler<GetItemsMasterByIdsQuery, List<GetItemAutoCompleteDto>>
    {
        private readonly IItemQueryRepository _itemQueryRepository;

        public GetItemsMasterByIdsQueryHandler(IItemQueryRepository itemQueryRepository)
        {
            _itemQueryRepository = itemQueryRepository;
        }

        public async Task<List<GetItemAutoCompleteDto>> Handle(
            GetItemsMasterByIdsQuery request,
            CancellationToken cancellationToken)
        {
            var ids = request.Ids?.Distinct().ToList() ?? new List<int>();
            if (ids.Count == 0)
                return new List<GetItemAutoCompleteDto>();

            var items = await _itemQueryRepository.GetItemsMasterByIdsAsync(ids);
            return items;
        }
    }
}
