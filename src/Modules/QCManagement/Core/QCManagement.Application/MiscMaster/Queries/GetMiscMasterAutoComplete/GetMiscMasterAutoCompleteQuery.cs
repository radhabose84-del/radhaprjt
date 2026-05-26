using MediatR;
using QCManagement.Application.MiscMaster.Dto;

namespace QCManagement.Application.MiscMaster.Queries.GetMiscMasterAutoComplete
{
    public sealed record GetMiscMasterAutoCompleteQuery(string Term, string? MiscTypeCode)
        : IRequest<IReadOnlyList<MiscMasterLookupDto>>;
}
