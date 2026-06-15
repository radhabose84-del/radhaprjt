using FinanceManagement.Application.GlAccountMaster.Dto;
using MediatR;

namespace FinanceManagement.Application.GlAccountMaster.Queries.GetGlAccountMasterAutoComplete
{
    public sealed record GetGlAccountMasterAutoCompleteQuery(string Term, string? AccountTypeCode)
        : IRequest<IReadOnlyList<GlAccountMasterLookupDto>>;
}
