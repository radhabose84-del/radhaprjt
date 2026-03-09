using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IInvoice;
using SalesManagement.Application.Invoice.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.Invoice.Queries.GetInvoiceById
{
    public class GetInvoiceByIdQueryHandler : IRequestHandler<GetInvoiceByIdQuery, InvoiceHeaderDto>
    {
        private readonly IInvoiceQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetInvoiceByIdQueryHandler(IInvoiceQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<InvoiceHeaderDto> Handle(GetInvoiceByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null!;

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetInvoiceByIdQuery",
                actionName: result.Id.ToString(),
                details: $"Invoice details {result.Id} was fetched.",
                module: "Invoice");
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
