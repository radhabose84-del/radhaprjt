using MediatR;

namespace SalesManagement.Application.Invoice.Commands.DeleteInvoice
{
    public sealed record DeleteInvoiceCommand(int Id) : IRequest<bool>;
}
