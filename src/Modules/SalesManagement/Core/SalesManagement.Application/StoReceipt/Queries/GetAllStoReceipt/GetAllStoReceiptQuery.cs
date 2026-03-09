using Contracts.Common;
using MediatR;
using SalesManagement.Application.StoReceipt.Dto;

namespace SalesManagement.Application.StoReceipt.Queries.GetAllStoReceipt
{
    public class GetAllStoReceiptQuery : IRequest<ApiResponseDTO<List<StoReceiptHeaderDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
