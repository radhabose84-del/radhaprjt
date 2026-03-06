using MediatR;
using SalesManagement.Application.StoHeader.Dto;

namespace SalesManagement.Application.StoHeader.Queries.GetStoHeaderAutoComplete
{
    public sealed record GetStoHeaderAutoCompleteQuery(string Term)
        : IRequest<IReadOnlyList<StoHeaderLookupDto>>;
}
