using Contracts.Dtos.Lookups.Production;
using MediatR;

namespace ProductionManagement.Application.CountMaster.Queries.GetCountMasterAutoComplete
{
    public sealed record GetCountMasterAutoCompleteQuery(string? Term)
        : IRequest<IReadOnlyList<CountMasterLookupDto>>;
}
