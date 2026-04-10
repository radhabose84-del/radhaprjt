using Contracts.Dtos.Lookups.Inventory;
using MediatR;

namespace InventoryManagement.Application.PriceGroupMaster.Queries.GetPriceGroupMasterAutoComplete
{
    public sealed record GetPriceGroupMasterAutoCompleteQuery(string Term)
        : IRequest<IReadOnlyList<PriceGroupMasterLookupDto>>;
}
