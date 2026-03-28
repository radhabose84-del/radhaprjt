using AutoMapper;
using Contracts.Dtos.Lookups.Production;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.IYarnTwistMaster;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.YarnTwistMaster.Queries.GetYarnTwistMasterAutoComplete
{
    public class GetYarnTwistMasterAutoCompleteQueryHandler : IRequestHandler<GetYarnTwistMasterAutoCompleteQuery, IReadOnlyList<YarnTwistMasterLookupDto>>
    {
        private readonly IYarnTwistMasterQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetYarnTwistMasterAutoCompleteQueryHandler(
            IYarnTwistMasterQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<YarnTwistMasterLookupDto>> Handle(GetYarnTwistMasterAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term ?? string.Empty, cancellationToken);
            var dtos = _mapper.Map<List<YarnTwistMasterLookupDto>>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetYarnTwistMasterAutoCompleteQuery",
                actionName: dtos.Count.ToString(),
                details: "Yarn Twist Master details was fetched.",
                module: "YarnTwistMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dtos;
        }
    }
}
