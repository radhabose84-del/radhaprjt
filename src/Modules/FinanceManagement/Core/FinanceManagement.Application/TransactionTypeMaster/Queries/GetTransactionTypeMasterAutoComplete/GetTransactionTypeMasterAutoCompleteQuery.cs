using FinanceManagement.Application.TransactionTypeMaster.Dto;
using MediatR;

namespace FinanceManagement.Application.TransactionTypeMaster.Queries.GetTransactionTypeMasterAutoComplete
{
    public sealed record GetTransactionTypeMasterAutoCompleteQuery(string Term)
        : IRequest<IReadOnlyList<TransactionTypeMasterLookupDto>>;
}
