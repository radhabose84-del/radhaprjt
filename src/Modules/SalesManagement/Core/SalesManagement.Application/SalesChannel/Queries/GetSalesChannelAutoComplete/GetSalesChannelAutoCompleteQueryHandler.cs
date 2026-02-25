using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesChannel;
using SalesManagement.Application.SalesChannel.Dto;

namespace SalesManagement.Application.SalesChannel.Queries.GetSalesChannelAutoComplete;

public sealed class GetSalesChannelAutoCompleteQueryHandler
    : IRequestHandler<GetSalesChannelAutoCompleteQuery, IReadOnlyList<SalesChannelLookupDto>>
{
    private readonly ISalesChannelQueryRepository _repo;

    public GetSalesChannelAutoCompleteQueryHandler(ISalesChannelQueryRepository repo) => _repo = repo;

    public Task<IReadOnlyList<SalesChannelLookupDto>> Handle(GetSalesChannelAutoCompleteQuery r, CancellationToken ct)
        => _repo.AutocompleteAsync(r.Term ?? string.Empty, ct);
}
