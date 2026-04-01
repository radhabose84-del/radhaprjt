using MediatR;
using ProductionManagement.Application.RepackingMaster.Dto;

namespace ProductionManagement.Application.RepackingMaster.Queries.GetRepackingMasterAutoComplete
{
    public sealed record GetRepackingMasterAutoCompleteQuery(string Term)
        : IRequest<IReadOnlyList<RepackingMasterLookupDto>>;
}
