using MediatR;

namespace SalesManagement.Application.TripSheet.Commands.DeleteTripSheet
{
    public sealed record DeleteTripSheetCommand(int Id) : IRequest<bool>;
}
