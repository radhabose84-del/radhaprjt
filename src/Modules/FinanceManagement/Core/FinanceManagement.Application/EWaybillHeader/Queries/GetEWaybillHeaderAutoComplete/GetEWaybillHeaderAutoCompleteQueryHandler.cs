using AutoMapper;
using FinanceManagement.Application.Common.Interfaces.IEWaybillHeader;
using FinanceManagement.Application.EWaybillHeader.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.EWaybillHeader.Queries.GetEWaybillHeaderAutoComplete
{
    public class GetEWaybillHeaderAutoCompleteQueryHandler : IRequestHandler<GetEWaybillHeaderAutoCompleteQuery, IReadOnlyList<EWaybillHeaderLookupDto>>
    {
        private readonly IEWaybillHeaderQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetEWaybillHeaderAutoCompleteQueryHandler(
            IEWaybillHeaderQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<EWaybillHeaderLookupDto>> Handle(GetEWaybillHeaderAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term ?? string.Empty, cancellationToken);
            var dtos = _mapper.Map<List<EWaybillHeaderLookupDto>>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetEWaybillHeaderAutoCompleteQuery",
                actionName: dtos.Count.ToString(),
                details: "EWaybill Header details was fetched.",
                module: "EWaybillHeader"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dtos;
        }
    }
}
