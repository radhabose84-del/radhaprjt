#nullable disable
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesGroup;
using SalesManagement.Application.SalesGroup.Dto;

namespace SalesManagement.Application.SalesGroup.Queries.GetSalesGroupAutoComplete;

public sealed class GetSalesGroupAutoCompleteQueryHandler
    : IRequestHandler<GetSalesGroupAutoCompleteQuery, IReadOnlyList<SalesGroupLookupDto>>
{
    private readonly ISalesGroupQueryRepository _repo;

    public GetSalesGroupAutoCompleteQueryHandler(ISalesGroupQueryRepository repo) => _repo = repo;

    public Task<IReadOnlyList<SalesGroupLookupDto>> Handle(GetSalesGroupAutoCompleteQuery r, CancellationToken ct)
        => _repo.AutocompleteAsync(r.Term ?? string.Empty, ct);
}
