using FinanceManagement.Application.TransactionTypeMaster.Dto;
using MediatR;

namespace FinanceManagement.Application.TransactionTypeMaster.Queries.GetTransactionTypeMasterById
{
    public class GetTransactionTypeMasterByIdQuery : IRequest<TransactionTypeMasterDto?>
    {
        public int Id { get; set; }
    }
}
