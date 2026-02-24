#nullable disable
using MediatR;
using SalesManagement.Application.MiscMaster.Dto;

namespace SalesManagement.Application.MiscMaster.Queries.GetMiscMasterAutoComplete
{
    public sealed record GetMiscMasterAutoCompleteQuery(string Term, int? MiscTypeId)
        : IRequest<IReadOnlyList<MiscMasterLookupDto>>;
}
