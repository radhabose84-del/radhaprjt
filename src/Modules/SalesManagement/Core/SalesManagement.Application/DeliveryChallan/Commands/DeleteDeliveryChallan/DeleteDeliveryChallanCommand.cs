using MediatR;

namespace SalesManagement.Application.DeliveryChallan.Commands.DeleteDeliveryChallan
{
    public sealed record DeleteDeliveryChallanCommand(int Id) : IRequest<bool>;
}
