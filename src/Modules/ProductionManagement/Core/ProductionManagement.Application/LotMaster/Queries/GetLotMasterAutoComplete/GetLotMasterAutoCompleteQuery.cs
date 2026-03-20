using MediatR;
using ProductionManagement.Application.LotMaster.Dto;

namespace ProductionManagement.Application.LotMaster.Queries.GetLotMasterAutoComplete
{
    public sealed record GetLotMasterAutoCompleteQuery(string? Term)
        : IRequest<IReadOnlyList<LotMasterLookupDto>>;
}
