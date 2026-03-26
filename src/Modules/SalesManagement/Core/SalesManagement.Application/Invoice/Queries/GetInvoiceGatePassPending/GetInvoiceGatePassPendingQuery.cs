using MediatR;

namespace SalesManagement.Application.Invoice.Queries.GetInvoiceGatePassPending
{
    public sealed class GetInvoiceGatePassPendingQuery
        : IRequest<List<GetInvoiceGatePassPendingDto>>
    {
    }
}
