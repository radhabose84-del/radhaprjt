using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IProformaInvoice;
using SalesManagement.Application.ProformaInvoice.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.ProformaInvoice.Queries.GetProformaInvoiceById
{
    public class GetProformaInvoiceByIdQueryHandler : IRequestHandler<GetProformaInvoiceByIdQuery, ProformaInvoiceDto?>
    {
        private readonly IProformaInvoiceQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetProformaInvoiceByIdQueryHandler(IProformaInvoiceQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ProformaInvoiceDto?> Handle(GetProformaInvoiceByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetProformaInvoiceByIdQuery",
                actionName: result.Id.ToString(),
                details: $"ProformaInvoice details {result.Id} was fetched.",
                module: "ProformaInvoice");
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
