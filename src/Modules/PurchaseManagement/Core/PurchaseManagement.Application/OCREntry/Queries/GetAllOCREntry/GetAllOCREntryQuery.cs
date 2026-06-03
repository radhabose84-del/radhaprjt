using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.OCREntry.Dto;

namespace PurchaseManagement.Application.OCREntry.Queries.GetAllOCREntry
{
    public class GetAllOCREntryQuery : IRequest<ApiResponseDTO<List<OCREntryDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
