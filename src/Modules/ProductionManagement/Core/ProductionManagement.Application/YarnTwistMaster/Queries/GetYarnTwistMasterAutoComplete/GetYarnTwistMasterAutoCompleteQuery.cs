using Contracts.Dtos.Lookups.Production;
using MediatR;

namespace ProductionManagement.Application.YarnTwistMaster.Queries.GetYarnTwistMasterAutoComplete
{
    public sealed record GetYarnTwistMasterAutoCompleteQuery(string? Term)
        : IRequest<IReadOnlyList<YarnTwistMasterLookupDto>>;
}
