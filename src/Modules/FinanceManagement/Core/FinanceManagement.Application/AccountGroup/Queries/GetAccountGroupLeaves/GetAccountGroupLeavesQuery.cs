using FinanceManagement.Application.AccountGroup.Dto;
using MediatR;

namespace FinanceManagement.Application.AccountGroup.Queries.GetAccountGroupLeaves
{
    // Assignable leaf groups for the GL-account "Account Group" picker. Company scope is resolved
    // from the session token in the handler; optionally narrowed to an account-type branch
    // (the leaf's L1 ancestor AccountTypeId).
    public sealed record GetAccountGroupLeavesQuery(int? AccountTypeId)
        : IRequest<IReadOnlyList<AccountGroupLookupDto>>;
}
