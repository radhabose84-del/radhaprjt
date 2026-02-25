using MediatR;
using SalesManagement.Application.SalesItemPriceMaster.Dto;

namespace SalesManagement.Application.SalesItemPriceMaster.Queries.GetSalesItemPriceMasterAutoComplete;

public sealed record GetSalesItemPriceMasterAutoCompleteQuery(string Term)
    : IRequest<IReadOnlyList<SalesItemPriceMasterLookupDto>>;
