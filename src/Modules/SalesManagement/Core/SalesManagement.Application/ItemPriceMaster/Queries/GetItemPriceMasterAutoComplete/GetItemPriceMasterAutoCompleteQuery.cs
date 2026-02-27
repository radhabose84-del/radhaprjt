using MediatR;
using SalesManagement.Application.ItemPriceMaster.Dto;

namespace SalesManagement.Application.ItemPriceMaster.Queries.GetItemPriceMasterAutoComplete;

public sealed record GetItemPriceMasterAutoCompleteQuery(string Term)
    : IRequest<IReadOnlyList<ItemPriceMasterLookupDto>>;
