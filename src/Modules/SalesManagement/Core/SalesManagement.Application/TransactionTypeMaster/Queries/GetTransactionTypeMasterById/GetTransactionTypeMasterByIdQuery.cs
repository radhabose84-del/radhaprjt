using MediatR;
using SalesManagement.Application.TransactionTypeMaster.Dto;

namespace SalesManagement.Application.TransactionTypeMaster.Queries.GetTransactionTypeMasterById
{
    public class GetTransactionTypeMasterByIdQuery : IRequest<TransactionTypeMasterDto?>
    {
        public int Id { get; set; }
    }
}
