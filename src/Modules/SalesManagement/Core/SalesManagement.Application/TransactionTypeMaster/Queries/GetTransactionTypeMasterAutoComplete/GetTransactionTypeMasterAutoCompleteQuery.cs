using MediatR;
using SalesManagement.Application.TransactionTypeMaster.Dto;

namespace SalesManagement.Application.TransactionTypeMaster.Queries.GetTransactionTypeMasterAutoComplete
{
    public sealed record GetTransactionTypeMasterAutoCompleteQuery(string Term)
        : IRequest<IReadOnlyList<TransactionTypeMasterLookupDto>>;
}
