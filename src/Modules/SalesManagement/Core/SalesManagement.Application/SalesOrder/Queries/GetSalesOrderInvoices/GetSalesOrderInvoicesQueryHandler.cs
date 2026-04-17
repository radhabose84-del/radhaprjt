using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesOrder;
using SalesManagement.Application.SalesOrder.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesOrder.Queries.GetSalesOrderInvoices
{
    public class GetSalesOrderInvoicesQueryHandler : IRequestHandler<GetSalesOrderInvoicesQuery, List<SalesOrderInvoiceDto>>
    {
        private readonly ISalesOrderQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetSalesOrderInvoicesQueryHandler(
            ISalesOrderQueryRepository queryRepository,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<List<SalesOrderInvoiceDto>> Handle(GetSalesOrderInvoicesQuery request, CancellationToken cancellationToken)
        {
            var data = await _queryRepository.GetSalesOrderInvoicesAsync(request.SalesOrderId);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetSalesOrderInvoicesQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: $"Invoices for Sales Order Id {request.SalesOrderId} were fetched.",
                module: "SalesOrder");
            await _mediator.Publish(domainEvent, cancellationToken);

            return data;
        }
    }
}
