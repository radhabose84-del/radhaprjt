using AutoMapper;
using BackgroundService.Application.Notification.Exceptions;
using BackgroundService.Application.Workflow.Common.Interfaces.IApprovalRule;
using BackgroundService.Domain.Entities.Workflow;
using BackgroundService.Domain.Events;
using MediatR;

namespace BackgroundService.Application.Workflow.ApprovalRules.Commands.DeleteApprovalRule
{
    public class DeleteApprovalRuleCommandHandler : IRequestHandler<DeleteApprovalRuleCommand, bool>
    {
        private readonly IApprovalRuleCommand _approvalRuleCommand;
        private readonly IMediator _imediator;
        private readonly IMapper _imapper;
        public DeleteApprovalRuleCommandHandler(IApprovalRuleCommand approvalRuleCommand, IMediator imediator, IMapper imapper)
        {
            _approvalRuleCommand = approvalRuleCommand;
            _imediator = imediator;
            _imapper = imapper;
        }
        public async Task<bool> Handle(DeleteApprovalRuleCommand request, CancellationToken cancellationToken)
        {
            var approvalRule = _imapper.Map<ApprovalRule>(request);
            var result = await _approvalRuleCommand.DeleteAsync(request.Id, approvalRule);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Delete",
                actionCode: "APPROVAL_RULE_DELETE",
                actionName: request.Id.ToString(),
                details: $"ApprovalRule with Id {request.Id} deleted successfully.",
                module: "ApprovalRule");
            await _imediator.Publish(domainEvent, cancellationToken);

            return result == true ? result : throw new ExceptionRules("Approval Rule deletion failed.");
        }
    }
}