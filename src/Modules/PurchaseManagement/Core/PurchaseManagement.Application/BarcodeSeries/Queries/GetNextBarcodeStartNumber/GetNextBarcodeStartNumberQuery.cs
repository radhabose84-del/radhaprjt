using MediatR;

namespace PurchaseManagement.Application.BarcodeSeries.Queries.GetNextBarcodeStartNumber
{
    public sealed record GetNextBarcodeStartNumberQuery : IRequest<long>;
}
