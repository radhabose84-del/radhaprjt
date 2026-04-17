using MediatR;
using SalesManagement.Application.Common.Interfaces.IInvoice;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.Invoice.Queries.GetDispatchTrackingDetails
{
    public class GetDispatchTrackingDetailsQueryHandler : IRequestHandler<GetDispatchTrackingDetailsQuery, DispatchTrackingDetailsDto?>
    {
        private readonly IInvoiceQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetDispatchTrackingDetailsQueryHandler(
            IInvoiceQueryRepository queryRepository,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<DispatchTrackingDetailsDto?> Handle(GetDispatchTrackingDetailsQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetDispatchTrackingDetailsAsync(request.SalesOrderId, cancellationToken);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetDispatchTrackingDetails",
                actionCode: "Get",
                actionName: request.SalesOrderId.ToString(),
                details: $"Dispatch tracking details for SalesOrder {request.SalesOrderId} were fetched.",
                module: "Invoice");
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
