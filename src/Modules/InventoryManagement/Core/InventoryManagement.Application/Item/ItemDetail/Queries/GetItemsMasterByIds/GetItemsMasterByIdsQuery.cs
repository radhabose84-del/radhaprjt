using InventoryManagement.Application.Item.ItemDetail.Queries.GetItemAutoComplete;
using MediatR;

namespace InventoryManagement.Application.Item.ItemDetail.Queries.GetItemsByIds
{
    public sealed class GetItemsMasterByIdsQuery : IRequest<List<GetItemAutoCompleteDto>>
    {
        public IEnumerable<int> Ids { get; }

        public GetItemsMasterByIdsQuery(IEnumerable<int> ids)
        {
            Ids = ids;
        }
    }
}
