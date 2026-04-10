using Contracts.Dtos.Lookups.Sales;
using MediatR;

namespace SalesManagement.Application.CommissionSplit.Queries.GetCommissionSplitAutoComplete
{
    public sealed record GetCommissionSplitAutoCompleteQuery(string Term)
        : IRequest<IReadOnlyList<CommissionSplitLookupDto>>;
}
