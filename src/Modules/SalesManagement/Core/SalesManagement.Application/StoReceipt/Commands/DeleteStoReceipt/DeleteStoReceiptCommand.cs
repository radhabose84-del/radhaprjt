using MediatR;

namespace SalesManagement.Application.StoReceipt.Commands.DeleteStoReceipt
{
    public sealed record DeleteStoReceiptCommand(int Id) : IRequest<bool>;
}
