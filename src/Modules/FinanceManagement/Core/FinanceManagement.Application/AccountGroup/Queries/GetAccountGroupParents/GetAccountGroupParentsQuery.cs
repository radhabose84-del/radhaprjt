using FinanceManagement.Application.AccountGroup.Dto;
using MediatR;

namespace FinanceManagement.Application.AccountGroup.Queries.GetAccountGroupParents
{
    // Eligible parents for the Move modal — active groups at the requested level
    // (typically the moved group's level minus one), optionally scoped to a company.
    public sealed record GetAccountGroupParentsQuery(int Level, int? CompanyId)
        : IRequest<IReadOnlyList<AccountGroupLookupDto>>;
}
