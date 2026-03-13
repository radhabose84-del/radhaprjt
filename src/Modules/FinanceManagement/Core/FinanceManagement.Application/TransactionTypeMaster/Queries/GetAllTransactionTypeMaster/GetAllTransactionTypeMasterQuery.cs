using Contracts.Common;
using FinanceManagement.Application.TransactionTypeMaster.Dto;
using MediatR;

namespace FinanceManagement.Application.TransactionTypeMaster.Queries.GetAllTransactionTypeMaster
{
    public class GetAllTransactionTypeMasterQuery : IRequest<ApiResponseDTO<List<TransactionTypeMasterDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
