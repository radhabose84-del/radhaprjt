using Contracts.Dtos.Lookups.Production;
using MediatR;

namespace ProductionManagement.Application.ProcessMaster.Queries.GetProcessMasterAutoComplete
{
    public sealed record GetProcessMasterAutoCompleteQuery(string? Term)
        : IRequest<IReadOnlyList<ProcessMasterLookupDto>>;
}
