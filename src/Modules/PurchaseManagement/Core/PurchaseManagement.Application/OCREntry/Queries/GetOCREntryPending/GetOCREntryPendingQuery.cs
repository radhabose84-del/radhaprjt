using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.OCREntry.Dto;

namespace PurchaseManagement.Application.OCREntry.Queries.GetOCREntryPending
{
    public class GetOCREntryPendingQuery : IRequest<ApiResponseDTO<List<OCREntryDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
