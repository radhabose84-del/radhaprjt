using MediatR;

namespace PurchaseManagement.Application.BarcodeSeries.Queries.GetNextBarcodeSeriesNumber
{
    public class GetNextBarcodeSeriesNumberQuery : IRequest<string>
    {
        public DateTimeOffset GenerationDate { get; set; }
    }
}
