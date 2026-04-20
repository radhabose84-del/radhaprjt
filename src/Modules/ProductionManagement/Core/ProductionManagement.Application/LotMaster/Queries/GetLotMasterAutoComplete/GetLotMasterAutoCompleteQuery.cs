using Contracts.Dtos.Lookups.Production;
using MediatR;

namespace ProductionManagement.Application.LotMaster.Queries.GetLotMasterAutoComplete
{
    public sealed record GetLotMasterAutoCompleteQuery(string? Term, int? ItemId = null, int? UnitId = null)
        : IRequest<IReadOnlyList<LotMasterLookupDto>>;
}
