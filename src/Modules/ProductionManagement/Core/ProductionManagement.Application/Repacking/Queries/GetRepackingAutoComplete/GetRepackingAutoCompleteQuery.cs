using MediatR;
using ProductionManagement.Application.Repacking.Dto;

namespace ProductionManagement.Application.Repacking.Queries.GetRepackingAutoComplete
{
    public sealed record GetRepackingAutoCompleteQuery(string? Term)
        : IRequest<IReadOnlyList<RepackingLookupDto>>;
}
