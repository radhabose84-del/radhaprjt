using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IAgentCommissionConfig;
using SalesManagement.Domain.Entities;
using SalesManagement.Domain.Events;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Application.AgentCommissionConfig.Commands.CreateAgentCommissionConfig
{
    public class CreateAgentCommissionConfigCommandHandler
        : IRequestHandler<CreateAgentCommissionConfigCommand, ApiResponseDTO<int>>
    {
        private readonly IAgentCommissionConfigCommandRepository _commandRepository;
        private readonly IAgentCommissionConfigQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateAgentCommissionConfigCommandHandler(
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
            CreateAgentCommissionConfigCommand request,
            CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.AgentCommissionConfig>(request);

            // Build child collections
            if (request.SalesGroupIds != null && request.SalesGroupIds.Any())
            {
                entity.AgentCommissionSalesGroups = request.SalesGroupIds.Select(sgId =>
                    new AgentCommissionSalesGroup
                    {
                        SalesGroupId = sgId,
                        IsActive = Status.Active,
                        IsDeleted = IsDelete.NotDeleted
                    }).ToList();
            }

            if (request.PaymentTermIds != null && request.PaymentTermIds.Any())
            {
                entity.AgentCommissionPaymentTerms = request.PaymentTermIds.Select(ptId =>
                    new AgentCommissionPaymentTerm
                    {
                        PaymentTermId = ptId,
                        IsActive = Status.Active,
                        IsDeleted = IsDelete.NotDeleted
                    }).ToList();
            }

            if (request.Slabs != null && request.Slabs.Any())
            {
                entity.AgentCommissionSlabs = request.Slabs.Select(s =>
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
                    }).ToList();
            }

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "AGENT_COMMISSION_CONFIG_CREATE",
                actionName: newId.ToString(),
                details: $"Agent Commission Configuration created successfully with Id {newId}.",
                module: "AgentCommissionConfig"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Agent Commission Configuration created successfully.",
                Data = newId
            };
        }
    }
}
