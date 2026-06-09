using MediatR;

namespace PurchaseManagement.Application.Arrival.Commands.DeleteArrival
{
    public sealed record DeleteArrivalCommand(int Id) : IRequest<bool>;
}
