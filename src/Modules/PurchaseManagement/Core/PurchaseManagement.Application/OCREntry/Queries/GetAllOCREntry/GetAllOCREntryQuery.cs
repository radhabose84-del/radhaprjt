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

        // Status filter (StatusId). null = no filter.
        public int? StatusId { get; set; }

        // OcrDate range filter (inclusive). null = no bound.
        public DateTimeOffset? FromDate { get; set; }
        public DateTimeOffset? ToDate { get; set; }
    }
}
