using MediatR;
using LogisticsManagement.Application.MiscMaster.Dto;

namespace LogisticsManagement.Application.MiscMaster.Queries.GetMiscMasterAutoComplete
{
    public sealed record GetMiscMasterAutoCompleteQuery(string Term)
        : IRequest<IReadOnlyList<MiscMasterLookupDto>>;
}
