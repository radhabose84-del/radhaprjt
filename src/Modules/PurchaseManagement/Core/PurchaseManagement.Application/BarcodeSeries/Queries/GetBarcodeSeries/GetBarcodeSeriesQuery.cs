using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.BarcodeSeries.Dto;

namespace PurchaseManagement.Application.BarcodeSeries.Queries.GetBarcodeSeries
{
    public class GetBarcodeSeriesQuery : IRequest<ApiResponseDTO<List<BarcodeSeriesDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }
    }
}
