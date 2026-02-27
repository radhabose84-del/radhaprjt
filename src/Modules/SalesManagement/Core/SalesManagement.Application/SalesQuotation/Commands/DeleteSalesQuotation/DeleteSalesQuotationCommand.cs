using MediatR;

namespace SalesManagement.Application.SalesQuotation.Commands.DeleteSalesQuotation
{
    public sealed record DeleteSalesQuotationCommand(int Id) : IRequest<bool>;
}
