using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IStoReceipt;
using SalesManagement.Application.StoReceipt.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.StoReceipt.Queries.GetStoReceiptAutoComplete
{
    public class GetStoReceiptAutoCompleteQueryHandler : IRequestHandler<GetStoReceiptAutoCompleteQuery, IReadOnlyList<StoReceiptLookupDto>>
    {
        private readonly IStoReceiptQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetStoReceiptAutoCompleteQueryHandler(
            IStoReceiptQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<StoReceiptLookupDto>> Handle(GetStoReceiptAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term ?? string.Empty, cancellationToken);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetStoReceiptAutoCompleteQuery",
                actionName: result.Count.ToString(),
                details: "StoReceipt details was fetched.",
                module: "StoReceipt");
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
