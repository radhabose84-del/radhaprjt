using MediatR;
using ProductionManagement.Application.CountMaster.Dto;

namespace ProductionManagement.Application.CountMaster.Queries.GetCountMasterAutoComplete
{
    public sealed record GetCountMasterAutoCompleteQuery(string? Term)
        : IRequest<IReadOnlyList<CountMasterLookupDto>>;
}
