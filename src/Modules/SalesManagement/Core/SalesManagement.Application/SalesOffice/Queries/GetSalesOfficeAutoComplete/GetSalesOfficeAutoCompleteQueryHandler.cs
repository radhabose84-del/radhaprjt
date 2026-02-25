using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesOffice;
using SalesManagement.Application.SalesOffice.Dto;

namespace SalesManagement.Application.SalesOffice.Queries.GetSalesOfficeAutoComplete;

public sealed class GetSalesOfficeAutoCompleteQueryHandler
    : IRequestHandler<GetSalesOfficeAutoCompleteQuery, IReadOnlyList<SalesOfficeLookupDto>>
{
    private readonly ISalesOfficeQueryRepository _repo;

    public GetSalesOfficeAutoCompleteQueryHandler(ISalesOfficeQueryRepository repo) => _repo = repo;

    public Task<IReadOnlyList<SalesOfficeLookupDto>> Handle(GetSalesOfficeAutoCompleteQuery r, CancellationToken ct)
        => _repo.AutocompleteAsync(r.Term ?? string.Empty, ct);
}
