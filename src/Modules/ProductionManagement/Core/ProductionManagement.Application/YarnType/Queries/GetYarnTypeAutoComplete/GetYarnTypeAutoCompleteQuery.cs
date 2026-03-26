using Contracts.Dtos.Lookups.Production;
using MediatR;

namespace ProductionManagement.Application.YarnType.Queries.GetYarnTypeAutoComplete
{
    public sealed record GetYarnTypeAutoCompleteQuery(string? Term)
        : IRequest<IReadOnlyList<YarnTypeLookupDto>>;
}
