using Contracts.Common;
using MediatR;
using SalesManagement.Application.TransactionTypeMaster.Dto;

namespace SalesManagement.Application.TransactionTypeMaster.Queries.GetAllTransactionTypeMaster
{
    public class GetAllTransactionTypeMasterQuery : IRequest<ApiResponseDTO<List<TransactionTypeMasterDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
