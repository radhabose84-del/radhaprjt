using PurchaseManagement.Application.Common.Interfaces.IPortMaster;
using MediatR;

namespace PurchaseManagement.Application.Port.Queries.GetPortAutocomplete;

public sealed class GetPortAutocompleteQueryHandler
    : IRequestHandler<GetPortAutocompleteQuery, IReadOnlyList<PortLookupDto>>
{
    private readonly IPortMasterQueryRepository _repo;
    public GetPortAutocompleteQueryHandler(IPortMasterQueryRepository repo) => _repo = repo;

    public Task<IReadOnlyList<PortLookupDto>> Handle(GetPortAutocompleteQuery r, CancellationToken ct)
        => _repo.AutocompleteAsync(r.Term ?? string.Empty, ct);
}
