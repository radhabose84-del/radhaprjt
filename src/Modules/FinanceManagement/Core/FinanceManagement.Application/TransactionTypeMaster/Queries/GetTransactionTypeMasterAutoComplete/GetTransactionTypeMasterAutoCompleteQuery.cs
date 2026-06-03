using FinanceManagement.Application.TransactionTypeMaster.Dto;
using MediatR;

namespace FinanceManagement.Application.TransactionTypeMaster.Queries.GetTransactionTypeMasterAutoComplete
{
    public sealed record GetTransactionTypeMasterAutoCompleteQuery(string Term, int? ModuleId = null, int? MenuId = null)
        : IRequest<IReadOnlyList<TransactionTypeMasterLookupDto>>;
}
