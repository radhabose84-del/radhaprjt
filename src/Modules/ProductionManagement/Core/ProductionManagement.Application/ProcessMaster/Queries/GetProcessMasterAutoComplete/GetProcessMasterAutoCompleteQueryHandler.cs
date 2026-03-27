using AutoMapper;
using Contracts.Dtos.Lookups.Production;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.IProcessMaster;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.ProcessMaster.Queries.GetProcessMasterAutoComplete
{
    public class GetProcessMasterAutoCompleteQueryHandler : IRequestHandler<GetProcessMasterAutoCompleteQuery, IReadOnlyList<ProcessMasterLookupDto>>
    {
        private readonly IProcessMasterQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetProcessMasterAutoCompleteQueryHandler(
            IProcessMasterQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<ProcessMasterLookupDto>> Handle(GetProcessMasterAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term ?? string.Empty, cancellationToken);
            var dtos = _mapper.Map<List<ProcessMasterLookupDto>>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetProcessMasterAutoCompleteQuery",
                actionName: dtos.Count.ToString(),
                details: "Process Master details was fetched.",
                module: "ProcessMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dtos;
        }
    }
}
