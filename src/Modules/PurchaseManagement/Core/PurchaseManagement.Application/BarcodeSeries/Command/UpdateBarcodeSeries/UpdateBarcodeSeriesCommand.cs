using Contracts.Common;
using MediatR;

namespace PurchaseManagement.Application.BarcodeSeries.Command.UpdateBarcodeSeries
{
    public class UpdateBarcodeSeriesCommand : IRequest<ApiResponseDTO<int>>
    {
        public int Id { get; set; }
        public int PrefixId { get; set; }
        public long BarcodeStartNumber { get; set; }
        public long BarcodeEndNumber { get; set; }
        public string? Remarks { get; set; }
        public int IsActive { get; set; }
    }
}
