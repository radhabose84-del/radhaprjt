using AutoMapper;
using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournalThresholdRule;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.JournalThresholdRule.Commands.UpdateJournalThresholdRule
{
    public class UpdateJournalThresholdRuleCommandHandler : IRequestHandler<UpdateJournalThresholdRuleCommand, ApiResponseDTO<int>>
    {
        private readonly IJournalThresholdRuleCommandRepository _commandRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateJournalThresholdRuleCommandHandler(
            IJournalThresholdRuleCommandRepository commandRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateJournalThresholdRuleCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<FinanceManagement.Domain.Entities.JournalThresholdRule>(request);

            var updatedId = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "JOURNAL_THRESHOLD_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Journal threshold rule with Id {request.Id} updated successfully.",
                module: "JournalThresholdRule"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Journal threshold rule updated successfully.",
                Data = updatedId
            };
        }
    }
}
