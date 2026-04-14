using MediatR;

namespace SalesManagement.Application.ProformaInvoice.Commands.DeleteProformaInvoice;

public sealed record DeleteProformaInvoiceCommand(int Id) : IRequest<bool>;
