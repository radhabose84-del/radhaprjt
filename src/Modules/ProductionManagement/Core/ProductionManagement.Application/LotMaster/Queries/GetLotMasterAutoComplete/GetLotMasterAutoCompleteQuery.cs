using Contracts.Dtos.Lookups.Production;
using MediatR;

namespace ProductionManagement.Application.LotMaster.Queries.GetLotMasterAutoComplete
{
    public sealed record GetLotMasterAutoCompleteQuery(string? Term)
        : IRequest<IReadOnlyList<LotMasterLookupDto>>;
}
