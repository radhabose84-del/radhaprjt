using MediatR;

namespace SalesManagement.Application.SalesLead.Commands.DeleteSalesLead
{
    public sealed record DeleteSalesLeadCommand(int Id) : IRequest<bool>;
}
