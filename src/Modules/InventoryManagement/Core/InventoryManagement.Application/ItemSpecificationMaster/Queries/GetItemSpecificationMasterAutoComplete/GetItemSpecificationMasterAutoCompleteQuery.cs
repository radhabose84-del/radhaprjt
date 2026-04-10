
using InventoryManagement.Application.ItemSpecificationMaster.Dto;
using MediatR;

namespace InventoryManagement.Application.ItemSpecificationMaster.Queries.GetItemSpecificationMasterAutoComplete
{
    public sealed record GetItemSpecificationMasterAutoCompleteQuery(string Term) : IRequest<IReadOnlyList<ItemSpecificationMasterLookupDto>>;
}
