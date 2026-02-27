using MediatR;
using SalesManagement.Application.MiscMaster.Dto;

namespace SalesManagement.Application.MiscMaster.Queries.GetMiscMasterAutoComplete
{
    public sealed record GetMiscMasterAutoCompleteQuery(string Term, string? MiscTypeCode)
        : IRequest<IReadOnlyList<MiscMasterLookupDto>>;
}
