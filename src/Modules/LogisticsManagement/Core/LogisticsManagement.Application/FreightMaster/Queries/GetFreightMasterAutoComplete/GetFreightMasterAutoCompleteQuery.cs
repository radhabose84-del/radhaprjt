using MediatR;
using LogisticsManagement.Application.FreightMaster.Dto;

namespace LogisticsManagement.Application.FreightMaster.Queries.GetFreightMasterAutoComplete
{
    public sealed record GetFreightMasterAutoCompleteQuery(string Term, int? ModuleId = null)
        : IRequest<IReadOnlyList<FreightMasterLookupDto>>;
}
