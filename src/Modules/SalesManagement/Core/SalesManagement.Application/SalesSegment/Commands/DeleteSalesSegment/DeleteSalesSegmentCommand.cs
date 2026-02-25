
using MediatR;

namespace SalesManagement.Application.SalesSegment.Commands.DeleteSalesSegment
{
    public sealed record DeleteSalesSegmentCommand(int Id) : IRequest<bool>;
}
