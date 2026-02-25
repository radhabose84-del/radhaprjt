using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesOrganisation;
using SalesManagement.Application.SalesOrganisation.Dto;

namespace SalesManagement.Application.SalesOrganisation.Queries.GetSalesOrganisationAutoComplete;

public sealed class GetSalesOrganisationAutoCompleteQueryHandler
    : IRequestHandler<GetSalesOrganisationAutoCompleteQuery, IReadOnlyList<SalesOrganisationLookupDto>>
{
    private readonly ISalesOrganisationQueryRepository _repo;

    public GetSalesOrganisationAutoCompleteQueryHandler(ISalesOrganisationQueryRepository repo) => _repo = repo;

    public Task<IReadOnlyList<SalesOrganisationLookupDto>> Handle(GetSalesOrganisationAutoCompleteQuery r, CancellationToken ct)
        => _repo.AutocompleteAsync(r.Term ?? string.Empty, ct);
}
