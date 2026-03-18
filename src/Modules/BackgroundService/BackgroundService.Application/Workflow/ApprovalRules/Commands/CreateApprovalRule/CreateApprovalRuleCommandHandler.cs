using AutoMapper;
using BackgroundService.Application.Notification.Exceptions;
using BackgroundService.Application.Workflow.Common.Interfaces.IApprovalRule;
using BackgroundService.Domain.Entities.Workflow;
using BackgroundService.Domain.Events;
using MediatR;

namespace BackgroundService.Application.Workflow.ApprovalRules.Commands.CreateApprovalRule
{
    public class CreateApprovalRuleCommandHandler : IRequestHandler<CreateApprovalRuleCommand, int>
    {
        private readonly IApprovalRuleCommand _approvalRuleCommand;
        private readonly IMediator _imediator;
        private readonly IMapper _imapper;

        public CreateApprovalRuleCommandHandler(IApprovalRuleCommand approvalRuleCommand, IMediator imediator, IMapper imapper)
        {
            _approvalRuleCommand = approvalRuleCommand;
            _imediator = imediator;
            _imapper = imapper;
        }

        public async Task<int> Handle(CreateApprovalRuleCommand request, CancellationToken cancellationToken)
        {
            var approvalRule = _imapper.Map<ApprovalRule>(request);
            var result = await _approvalRuleCommand.CreateAsync(approvalRule);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "APPROVAL_RULE_CREATE",
                actionName: request.ApprovalStepDetailId.ToString(),
                details: $"ApprovalRule created for ApprovalStepDetailId {request.ApprovalStepDetailId} with Priority {request.Priority}.",
                module: "ApprovalRule");
            await _imediator.Publish(domainEvent, cancellationToken);

            return result > 0 ? result : throw new ExceptionRules("Approval Rule Creation Failed.");
        }
    }
}