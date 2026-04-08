using MediatR;
using ProductionManagement.Application.RepackingHeader.Dto;

namespace ProductionManagement.Application.RepackingHeader.Queries.GetRepackingHeaderAutoComplete
{
    public sealed record GetRepackingHeaderAutoCompleteQuery(string? Term)
        : IRequest<IReadOnlyList<RepackingHeaderLookupDto>>;
}
