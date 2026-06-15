using FinanceManagement.Application.MiscMaster.Dto;
using MediatR;

namespace FinanceManagement.Application.MiscMaster.Queries.GetMiscMasterAutoComplete
{
    public sealed record GetMiscMasterAutoCompleteQuery(string Term, string? MiscTypeCode)
        : IRequest<IReadOnlyList<MiscMasterLookupDto>>;
}
