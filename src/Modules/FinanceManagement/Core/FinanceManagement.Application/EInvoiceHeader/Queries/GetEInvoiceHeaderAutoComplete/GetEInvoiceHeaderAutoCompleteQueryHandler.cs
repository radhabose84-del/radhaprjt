using AutoMapper;
using FinanceManagement.Application.Common.Interfaces.IEInvoiceHeader;
using FinanceManagement.Application.EInvoiceHeader.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.EInvoiceHeader.Queries.GetEInvoiceHeaderAutoComplete
{
    public class GetEInvoiceHeaderAutoCompleteQueryHandler : IRequestHandler<GetEInvoiceHeaderAutoCompleteQuery, IReadOnlyList<EInvoiceHeaderLookupDto>>
    {
        private readonly IEInvoiceHeaderQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetEInvoiceHeaderAutoCompleteQueryHandler(
            IEInvoiceHeaderQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<EInvoiceHeaderLookupDto>> Handle(GetEInvoiceHeaderAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term ?? string.Empty, cancellationToken);
            var dtos = _mapper.Map<List<EInvoiceHeaderLookupDto>>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetEInvoiceHeaderAutoCompleteQuery",
                actionName: dtos.Count.ToString(),
                details: "EInvoice Header details was fetched.",
                module: "EInvoiceHeader"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dtos;
        }
    }
}
