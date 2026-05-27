using AutoMapper;
using Contracts.Dtos.Lookups.Purchase;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IDeliveryScoreRule;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.DeliveryScoreRule.Queries.GetDeliveryScoreRuleAutoComplete
{
    public class GetDeliveryScoreRuleAutoCompleteQueryHandler : IRequestHandler<GetDeliveryScoreRuleAutoCompleteQuery, IReadOnlyList<DeliveryScoreRuleLookupDto>>
    {
        private readonly IDeliveryScoreRuleQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetDeliveryScoreRuleAutoCompleteQueryHandler(IDeliveryScoreRuleQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<DeliveryScoreRuleLookupDto>> Handle(GetDeliveryScoreRuleAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term ?? string.Empty, cancellationToken);
            var dtos = _mapper.Map<List<DeliveryScoreRuleLookupDto>>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetDeliveryScoreRuleAutoCompleteQuery",
                actionName: dtos.Count.ToString(),
                details: "DeliveryScoreRule details was fetched.",
                module: "DeliveryScoreRule"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dtos;
        }
    }
}
