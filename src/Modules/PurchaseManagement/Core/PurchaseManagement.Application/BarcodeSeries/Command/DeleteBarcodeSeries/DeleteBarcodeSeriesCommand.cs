using MediatR;

namespace PurchaseManagement.Application.BarcodeSeries.Command.DeleteBarcodeSeries
{
    public sealed record DeleteBarcodeSeriesCommand(int Id) : IRequest<bool>;
}
