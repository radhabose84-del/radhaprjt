using AutoMapper;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.IRepackingMaster;
using ProductionManagement.Application.RepackingMaster.Dto;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.RepackingMaster.Queries.GetRepackingMasterAutoComplete
{
    public class GetRepackingMasterAutoCompleteQueryHandler
        : IRequestHandler<GetRepackingMasterAutoCompleteQuery, IReadOnlyList<RepackingMasterLookupDto>>
    {
        private readonly IRepackingMasterQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetRepackingMasterAutoCompleteQueryHandler(
            IRepackingMasterQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<RepackingMasterLookupDto>> Handle(
            GetRepackingMasterAutoCompleteQuery request,
            CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term ?? string.Empty, cancellationToken);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetRepackingMasterAutoCompleteQuery",
                actionName: result.Count.ToString(),
                details: "RepackingMaster details was fetched.",
                module: "RepackingMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
