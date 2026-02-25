using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesItemPriceMaster;
using SalesManagement.Application.SalesItemPriceMaster.Dto;

namespace SalesManagement.Application.SalesItemPriceMaster.Queries.GetSalesItemPriceMasterAutoComplete;

public sealed class GetSalesItemPriceMasterAutoCompleteQueryHandler
    : IRequestHandler<GetSalesItemPriceMasterAutoCompleteQuery, IReadOnlyList<SalesItemPriceMasterLookupDto>>
{
    private readonly ISalesItemPriceMasterQueryRepository _repo;

    public GetSalesItemPriceMasterAutoCompleteQueryHandler(ISalesItemPriceMasterQueryRepository repo)
        => _repo = repo;

    public Task<IReadOnlyList<SalesItemPriceMasterLookupDto>> Handle(
        GetSalesItemPriceMasterAutoCompleteQuery r, CancellationToken ct)
        => _repo.AutocompleteAsync(r.Term ?? string.Empty, ct);
}
