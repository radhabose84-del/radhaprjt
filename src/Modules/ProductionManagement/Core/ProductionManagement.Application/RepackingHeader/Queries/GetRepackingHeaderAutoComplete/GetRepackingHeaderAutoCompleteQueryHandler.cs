using AutoMapper;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.IRepackingHeader;
using ProductionManagement.Application.RepackingHeader.Dto;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.RepackingHeader.Queries.GetRepackingHeaderAutoComplete
{
    public class GetRepackingHeaderAutoCompleteQueryHandler
        : IRequestHandler<GetRepackingHeaderAutoCompleteQuery, IReadOnlyList<RepackingHeaderLookupDto>>
    {
        private readonly IRepackingHeaderQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetRepackingHeaderAutoCompleteQueryHandler(
            IRepackingHeaderQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<RepackingHeaderLookupDto>> Handle(
            GetRepackingHeaderAutoCompleteQuery request,
            CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(
                request.Term ?? string.Empty, cancellationToken);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetRepackingHeaderAutoCompleteQuery",
                actionName: result.Count.ToString(),
                details: "RepackingHeader details was fetched.",
                module: "RepackingHeader"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
