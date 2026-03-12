using AutoMapper;
using FinanceManagement.Application.Common.Interfaces.IEInvoiceHeader;
using FinanceManagement.Application.EInvoiceHeader.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.EInvoiceHeader.Queries.GetEInvoiceHeaderById
{
    public class GetEInvoiceHeaderByIdQueryHandler : IRequestHandler<GetEInvoiceHeaderByIdQuery, EInvoiceHeaderDto?>
    {
        private readonly IEInvoiceHeaderQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetEInvoiceHeaderByIdQueryHandler(
            IEInvoiceHeaderQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<EInvoiceHeaderDto?> Handle(GetEInvoiceHeaderByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            var dto = _mapper.Map<EInvoiceHeaderDto>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetEInvoiceHeaderByIdQuery",
                actionName: dto.Id.ToString(),
                details: $"EInvoice Header details {dto.Id} was fetched.",
                module: "EInvoiceHeader"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dto;
        }
    }
}
