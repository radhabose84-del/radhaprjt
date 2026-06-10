using MediatR;

namespace PurchaseManagement.Application.FreightRfq.Commands.DeleteFreightRfq
{
    public sealed record DeleteFreightRfqCommand(int Id) : IRequest<bool>;
}
