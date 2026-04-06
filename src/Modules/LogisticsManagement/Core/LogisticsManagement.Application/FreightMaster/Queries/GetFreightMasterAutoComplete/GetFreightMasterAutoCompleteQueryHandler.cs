using AutoMapper;
using MediatR;
using LogisticsManagement.Application.Common.Interfaces.IFreightMaster;
using LogisticsManagement.Application.FreightMaster.Dto;
using LogisticsManagement.Domain.Events;

namespace LogisticsManagement.Application.FreightMaster.Queries.GetFreightMasterAutoComplete
{
    public class GetFreightMasterAutoCompleteQueryHandler : IRequestHandler<GetFreightMasterAutoCompleteQuery, IReadOnlyList<FreightMasterLookupDto>>
    {
        private readonly IFreightMasterQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetFreightMasterAutoCompleteQueryHandler(IFreightMasterQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<FreightMasterLookupDto>> Handle(GetFreightMasterAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term ?? string.Empty, cancellationToken);
            var dtos = _mapper.Map<List<FreightMasterLookupDto>>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetFreightMasterAutoCompleteQuery",
                actionName: dtos.Count.ToString(),
                details: "FreightMaster details was fetched.",
                module: "FreightMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dtos;
        }
    }
}
