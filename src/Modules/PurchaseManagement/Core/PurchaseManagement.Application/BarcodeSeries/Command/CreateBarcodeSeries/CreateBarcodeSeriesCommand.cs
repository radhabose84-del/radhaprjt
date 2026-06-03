using Contracts.Common;
using MediatR;

namespace PurchaseManagement.Application.BarcodeSeries.Command.CreateBarcodeSeries
{
    public class CreateBarcodeSeriesCommand : IRequest<ApiResponseDTO<int>>
    {
        public int PrefixId { get; set; }
        public long BarcodeStartNumber { get; set; }
        public long BarcodeEndNumber { get; set; }
        public DateTimeOffset GenerationDate { get; set; }
        public string? Remarks { get; set; }
    }
}
