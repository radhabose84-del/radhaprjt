using MediatR;
using LogisticsManagement.Application.MiscTypeMaster.Dto;

namespace LogisticsManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMasterAutoComplete
{
    public sealed record GetMiscTypeMasterAutoCompleteQuery(string Term)
        : IRequest<IReadOnlyList<MiscTypeMasterLookupDto>>;
}
