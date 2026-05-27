using AutoMapper;
using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IDeliveryScoreRule;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.DeliveryScoreRule.Commands.UpdateDeliveryScoreRule
{
    public class UpdateDeliveryScoreRuleCommandHandler : IRequestHandler<UpdateDeliveryScoreRuleCommand, ApiResponseDTO<int>>
    {
        private readonly IDeliveryScoreRuleCommandRepository _commandRepository;
        private readonly IDeliveryScoreRuleQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateDeliveryScoreRuleCommandHandler(
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

        public async Task<ApiResponseDTO<int>> Handle(UpdateDeliveryScoreRuleCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.VendorEvaluation.DeliveryScoreRule>(request);
            var result = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "DELIVERY_SCORE_RULE_UPDATE",
                actionName: request.Id.ToString(),
                details: $"DeliveryScoreRule with Id {request.Id} updated successfully.",
                module: "DeliveryScoreRule"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "DeliveryScoreRule updated successfully.",
                Data = result
            };
        }
    }
}
