using FinanceManagement.Application.AccountGroup.Dto;
using MediatR;

namespace FinanceManagement.Application.AccountGroup.Queries.GetAccountGroupParents
{
    // Eligible parents for the Move modal — active groups at the requested level
    // (typically the moved group's level minus one). Company scope is resolved from the
    // session token in the handler.
    public sealed record GetAccountGroupParentsQuery(int Level)
        : IRequest<IReadOnlyList<AccountGroupLookupDto>>;
}
