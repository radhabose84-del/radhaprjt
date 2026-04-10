
using InventoryManagement.Application.ItemSpecificationValue.Dto;
using MediatR;

namespace InventoryManagement.Application.ItemSpecificationValue.Queries.GetItemSpecificationValueAutoComplete
{
    public sealed record GetItemSpecificationValueAutoCompleteQuery(string Term, int? SpecificationMasterId) : IRequest<IReadOnlyList<ItemSpecificationValueLookupDto>>;
}
