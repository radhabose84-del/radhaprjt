using FinanceManagement.Application.AccountGroup.Dto;
using MediatR;

namespace FinanceManagement.Application.AccountGroup.Queries.GetAccountGroupLeaves
{
    // Assignable leaf groups for the GL-account "Account Group" picker — optionally scoped to a
    // company and to an account-type branch (the leaf's L1 ancestor AccountTypeId).
    public sealed record GetAccountGroupLeavesQuery(int? CompanyId, int? AccountTypeId)
        : IRequest<IReadOnlyList<AccountGroupLookupDto>>;
}
