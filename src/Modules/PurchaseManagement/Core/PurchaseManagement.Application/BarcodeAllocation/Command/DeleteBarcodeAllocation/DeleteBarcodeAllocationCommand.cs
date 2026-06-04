using MediatR;

namespace PurchaseManagement.Application.BarcodeAllocation.Command.DeleteBarcodeAllocation
{
    public sealed record DeleteBarcodeAllocationCommand(int Id) : IRequest<bool>;
}
