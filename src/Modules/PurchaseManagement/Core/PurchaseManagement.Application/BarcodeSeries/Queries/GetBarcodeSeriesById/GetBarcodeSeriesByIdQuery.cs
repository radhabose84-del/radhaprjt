using MediatR;
using PurchaseManagement.Application.BarcodeSeries.Dto;

namespace PurchaseManagement.Application.BarcodeSeries.Queries.GetBarcodeSeriesById
{
    public class GetBarcodeSeriesByIdQuery : IRequest<BarcodeSeriesDto?>
    {
        public int Id { get; set; }
    }
}
