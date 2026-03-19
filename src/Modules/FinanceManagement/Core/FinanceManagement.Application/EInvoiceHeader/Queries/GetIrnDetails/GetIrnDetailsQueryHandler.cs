using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.IEInvoiceHeader;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.EInvoiceHeader.Queries.GetIrnDetails
{
    public class GetIrnDetailsQueryHandler
        : IRequestHandler<GetIrnDetailsQuery, ApiResponseDTO<object>>
    {
        private readonly INicEInvoiceService _nicEInvoiceService;
        private readonly IMediator _mediator;

        public GetIrnDetailsQueryHandler(
            INicEInvoiceService nicEInvoiceService,
            IMediator mediator)
        {
            _nicEInvoiceService = nicEInvoiceService;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<object>> Handle(
            GetIrnDetailsQuery request,
            CancellationToken cancellationToken)
        {
            var result = await _nicEInvoiceService.GetIrnDetailsAsync(
                request.EInvoiceHeaderId, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "GetIrnDetails",
                actionCode: "EINVOICE_GET_IRN_DETAILS",
                actionName: request.EInvoiceHeaderId.ToString(),
                details: $"IRN details fetched for EInvoiceHeader {request.EInvoiceHeaderId}",
                module: "EInvoiceHeader");
            await _mediator.Publish(auditEvent, cancellationToken);

            return result;
        }
    }
}
