using AutoMapper;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.ILotMaster;
using ProductionManagement.Application.LotMaster.Dto;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.LotMaster.Queries.GetLotMasterAutoComplete
{
    public class GetLotMasterAutoCompleteQueryHandler
        : IRequestHandler<GetLotMasterAutoCompleteQuery, IReadOnlyList<LotMasterLookupDto>>
    {
        private readonly ILotMasterQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetLotMasterAutoCompleteQueryHandler(
            ILotMasterQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<LotMasterLookupDto>> Handle(
            GetLotMasterAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(
                request.Term ?? string.Empty, cancellationToken);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetLotMasterAutoCompleteQuery",
                actionName: result.Count.ToString(),
                details: "LotMaster details was fetched.",
                module: "LotMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
