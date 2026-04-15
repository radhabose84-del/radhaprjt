using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDispatchAdvice;
using SalesManagement.Application.DispatchAdvice.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.DispatchAdvice.Queries.GetDispatchAdviceAutoComplete
{
    public class GetDispatchAdviceAutoCompleteQueryHandler : IRequestHandler<GetDispatchAdviceAutoCompleteQuery, IReadOnlyList<DispatchAdviceLookupDto>>
    {
        private readonly IDispatchAdviceQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetDispatchAdviceAutoCompleteQueryHandler(
            IDispatchAdviceQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<DispatchAdviceLookupDto>> Handle(GetDispatchAdviceAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term ?? string.Empty, cancellationToken, request.ProformaFilter);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetDispatchAdviceAutoCompleteQuery",
                actionName: result.Count.ToString(),
                details: "DispatchAdvice details was fetched.",
                module: "DispatchAdvice");
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
