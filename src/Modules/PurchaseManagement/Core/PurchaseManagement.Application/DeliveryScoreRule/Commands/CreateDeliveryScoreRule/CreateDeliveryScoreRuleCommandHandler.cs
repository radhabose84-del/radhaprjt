using AutoMapper;
using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IDeliveryScoreRule;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.DeliveryScoreRule.Commands.CreateDeliveryScoreRule
{
    public class CreateDeliveryScoreRuleCommandHandler : IRequestHandler<CreateDeliveryScoreRuleCommand, ApiResponseDTO<int>>
    {
        private readonly IDeliveryScoreRuleCommandRepository _commandRepository;
        private readonly IDeliveryScoreRuleQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateDeliveryScoreRuleCommandHandler(
            IDeliveryScoreRuleCommandRepository commandRepository,
            IDeliveryScoreRuleQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateDeliveryScoreRuleCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.VendorEvaluation.DeliveryScoreRule>(request);
            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "DELIVERY_SCORE_RULE_CREATE",
                actionName: request.RuleCode ?? string.Empty,
                details: $"DeliveryScoreRule '{request.RuleCode}' created successfully with Id {newId}.",
                module: "DeliveryScoreRule"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "DeliveryScoreRule created successfully.",
                Data = newId
            };
        }
    }
}
