using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IProformaInvoice;
using SalesManagement.Application.ProformaInvoice.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.ProformaInvoice.Queries.GetProformaInvoiceBySalesOrder
{
    public class GetProformaInvoiceBySalesOrderQueryHandler : IRequestHandler<GetProformaInvoiceBySalesOrderQuery, List<ProformaInvoiceDto>>
    {
        private readonly IProformaInvoiceQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetProformaInvoiceBySalesOrderQueryHandler(IProformaInvoiceQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<List<ProformaInvoiceDto>> Handle(GetProformaInvoiceBySalesOrderQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetBySalesOrderIdAsync(request.SalesOrderId);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetBySalesOrder",
                actionCode: "GetProformaInvoiceBySalesOrderQuery",
                actionName: result.Count.ToString(),
                details: $"ProformaInvoice details for SalesOrder {request.SalesOrderId} fetched.",
                module: "ProformaInvoice");
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
