using AutoMapper;
using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournalThresholdRule;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.JournalThresholdRule.Commands.CreateJournalThresholdRule
{
    public class CreateJournalThresholdRuleCommandHandler : IRequestHandler<CreateJournalThresholdRuleCommand, ApiResponseDTO<int>>
    {
        private readonly IJournalThresholdRuleCommandRepository _commandRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateJournalThresholdRuleCommandHandler(
            IJournalThresholdRuleCommandRepository commandRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateJournalThresholdRuleCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<FinanceManagement.Domain.Entities.JournalThresholdRule>(request);

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "JOURNAL_THRESHOLD_CREATE",
                actionName: newId.ToString(),
                details: $"Journal threshold rule created with Id {newId}.",
                module: "JournalThresholdRule"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Journal threshold rule created successfully.",
                Data = newId
            };
        }
    }
}
