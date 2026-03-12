using MediatR;

namespace FinanceManagement.Application.EInvoiceHeader.Commands.DeleteEInvoiceHeader
{
    public sealed record DeleteEInvoiceHeaderCommand(int Id) : IRequest<bool>;
}
