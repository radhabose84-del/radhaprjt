using MediatR;

namespace SalesManagement.Application.SalesOffice.Commands.DeleteSalesOffice
{
    public sealed record DeleteSalesOfficeCommand(int Id) : IRequest<bool>;
}
