using Contracts.Dtos.Lookups.Production;
using MediatR;

namespace ProductionManagement.Application.CountGroup.Queries.GetCountGroupAutoComplete
{
    public sealed record GetCountGroupAutoCompleteQuery(string? Term)
        : IRequest<IReadOnlyList<CountGroupLookupDto>>;
}
