using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IAgentCommissionConfig;
using SalesManagement.Domain.Entities;
using SalesManagement.Domain.Events;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Application.AgentCommissionConfig.Commands.UpdateAgentCommissionConfig
{
    public class UpdateAgentCommissionConfigCommandHandler
        : IRequestHandler<UpdateAgentCommissionConfigCommand, ApiResponseDTO<int>>
    {
        private readonly IAgentCommissionConfigCommandRepository _commandRepository;
        private readonly IAgentCommissionConfigQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateAgentCommissionConfigCommandHandler(
            IAgentCommissionConfigCommandRepository commandRepository,
            IAgentCommissionConfigQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(
            UpdateAgentCommissionConfigCommand request,
            CancellationToken cancellationToken)
        {
            // Inactivate guard (Rule 25)
            if (request.IsActive == 0)
            {
                var isLinked = await _queryRepository.IsAgentCommissionConfigLinkedAsync(request.Id);
                if (isLinked)
                    throw new ExceptionRules(
                        "This master is linked with other records. You cannot inactivate this record.");
            }

            var entity = _mapper.Map<Domain.Entities.AgentCommissionConfig>(request);

            // Build child collections (replace strategy)
            entity.AgentCommissionSalesGroups = request.SalesGroupIds?.Select(sgId =>
                new AgentCommissionSalesGroup
                {
                    SalesGroupId = sgId,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                }).ToList() ?? new List<AgentCommissionSalesGroup>();

            entity.AgentCommissionPaymentTerms = request.PaymentTermIds?.Select(ptId =>
                new AgentCommissionPaymentTerm
                {
                    PaymentTermId = ptId,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                }).ToList() ?? new List<AgentCommissionPaymentTerm>();

            entity.AgentCommissionSlabs = request.Slabs?.Select(s =>
                new AgentCommissionSlab
                {
                    SlabOrder = s.SlabOrder,
                    FromDelay = s.FromDelay,
                    ToDelay = s.ToDelay,
                    CommissionTypeId = s.CommissionTypeId,
                    CommissionBasisId = s.CommissionBasisId,
                    CommissionValue = s.CommissionValue,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                }).ToList() ?? new List<AgentCommissionSlab>();

            var result = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "AGENT_COMMISSION_CONFIG_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Agent Commission Configuration with Id {request.Id} updated successfully.",
                module: "AgentCommissionConfig"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Agent Commission Configuration updated successfully.",
                Data = result
            };
        }
    }
}
