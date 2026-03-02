using MediatR;
using SalesManagement.Application.LotMaster.Dto;

namespace SalesManagement.Application.LotMaster.Queries.GetLotMasterAutoComplete
{
    public sealed record GetLotMasterAutoCompleteQuery(string? Term)
        : IRequest<IReadOnlyList<LotMasterLookupDto>>;
}
