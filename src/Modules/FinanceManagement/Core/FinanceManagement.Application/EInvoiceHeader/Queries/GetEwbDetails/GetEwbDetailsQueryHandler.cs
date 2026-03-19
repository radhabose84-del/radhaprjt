using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.IEInvoiceHeader;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.EInvoiceHeader.Queries.GetEwbDetails
{
    public class GetEwbDetailsQueryHandler
        : IRequestHandler<GetEwbDetailsQuery, ApiResponseDTO<object>>
    {
        private readonly INicEInvoiceService _nicEInvoiceService;
        private readonly IMediator _mediator;

        public GetEwbDetailsQueryHandler(
            INicEInvoiceService nicEInvoiceService,
            IMediator mediator)
        {
            _nicEInvoiceService = nicEInvoiceService;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<object>> Handle(
            GetEwbDetailsQuery request,
            CancellationToken cancellationToken)
        {
            var result = await _nicEInvoiceService.GetEwbDetailsByIrnAsync(
                request.EInvoiceHeaderId, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "GetEwbDetails",
                actionCode: "EINVOICE_GET_EWB_DETAILS",
                actionName: request.EInvoiceHeaderId.ToString(),
                details: $"e-Waybill details fetched for EInvoiceHeader {request.EInvoiceHeaderId}",
                module: "EInvoiceHeader");
            await _mediator.Publish(auditEvent, cancellationToken);

            return result;
        }
    }
}
