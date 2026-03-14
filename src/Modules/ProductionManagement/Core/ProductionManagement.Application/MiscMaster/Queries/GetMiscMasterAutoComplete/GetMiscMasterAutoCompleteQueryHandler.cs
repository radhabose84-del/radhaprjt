using AutoMapper;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.IMiscMaster;
using ProductionManagement.Application.MiscMaster.Dto;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.MiscMaster.Queries.GetMiscMasterAutoComplete
{
    public class GetMiscMasterAutoCompleteQueryHandler : IRequestHandler<GetMiscMasterAutoCompleteQuery, IReadOnlyList<MiscMasterLookupDto>>
    {
        private readonly IMiscMasterQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetMiscMasterAutoCompleteQueryHandler(
            IMiscMasterQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<MiscMasterLookupDto>> Handle(GetMiscMasterAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(
                request.Term ?? string.Empty,
                request.MiscTypeCode,
                cancellationToken);

            var dtos = _mapper.Map<List<MiscMasterLookupDto>>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetMiscMasterAutoCompleteQuery",
                actionName: dtos.Count.ToString(),
                details: "Misc Master details was fetched.",
                module: "MiscMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dtos;
        }
    }
}
