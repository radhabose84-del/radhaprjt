using AutoMapper;
using Contracts.Dtos.Lookups.Production;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.IQualityMaster;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.QualityMaster.Queries.GetQualityMasterAutoComplete
{
    public class GetQualityMasterAutoCompleteQueryHandler : IRequestHandler<GetQualityMasterAutoCompleteQuery, IReadOnlyList<QualityMasterLookupDto>>
    {
        private readonly IQualityMasterQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetQualityMasterAutoCompleteQueryHandler(
            IQualityMasterQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<QualityMasterLookupDto>> Handle(GetQualityMasterAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term ?? string.Empty, cancellationToken);
            var dtos = _mapper.Map<List<QualityMasterLookupDto>>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetQualityMasterAutoCompleteQuery",
                actionName: dtos.Count.ToString(),
                details: "Quality Master details was fetched.",
                module: "QualityMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dtos;
        }
    }
}
