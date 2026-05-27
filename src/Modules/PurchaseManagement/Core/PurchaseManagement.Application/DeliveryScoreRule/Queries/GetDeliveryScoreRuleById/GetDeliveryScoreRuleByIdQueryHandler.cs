using AutoMapper;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IDeliveryScoreRule;
using PurchaseManagement.Application.DeliveryScoreRule.Dto;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.DeliveryScoreRule.Queries.GetDeliveryScoreRuleById
{
    public class GetDeliveryScoreRuleByIdQueryHandler : IRequestHandler<GetDeliveryScoreRuleByIdQuery, DeliveryScoreRuleDto?>
    {
        private readonly IDeliveryScoreRuleQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetDeliveryScoreRuleByIdQueryHandler(IDeliveryScoreRuleQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<DeliveryScoreRuleDto?> Handle(GetDeliveryScoreRuleByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);
            if (result == null) return null;

            var dto = _mapper.Map<DeliveryScoreRuleDto>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetDeliveryScoreRuleByIdQuery",
                actionName: dto.Id.ToString(),
                details: $"DeliveryScoreRule details {dto.Id} was fetched.",
                module: "DeliveryScoreRule"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dto;
        }
    }
}
