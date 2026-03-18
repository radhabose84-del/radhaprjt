using AutoMapper;
using BackgroundService.Application.Notification.Exceptions;
using BackgroundService.Application.Workflow.Common.Interfaces.IApprovalStepDetail;
using BackgroundService.Domain.Entities.Workflow;
using BackgroundService.Domain.Events;
using MediatR;

namespace BackgroundService.Application.Workflow.ApprovalStepDetails.Commands.DeleteApprovalStepDetail
{
    public class DeleteApprovalStepDetailCommandHandler : IRequestHandler<DeleteApprovalStepDetailCommand, bool>
    {
        private readonly IApprovalStepDetailCommand _approvalStepDetailCommand;
        private readonly IMediator _imediator;
        private readonly IMapper _imapper;

        public DeleteApprovalStepDetailCommandHandler(IApprovalStepDetailCommand approvalStepDetailCommand, IMediator imediator, IMapper imapper)
        {
            _approvalStepDetailCommand = approvalStepDetailCommand;
            _imediator = imediator;
            _imapper = imapper;
        }

        public async Task<bool> Handle(DeleteApprovalStepDetailCommand request, CancellationToken cancellationToken)
        {
            var approvalStep = _imapper.Map<ApprovalStepDetail>(request);
            var result = await _approvalStepDetailCommand.DeleteAsync(request.Id, approvalStep);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Delete",
                actionCode: "APPROVAL_STEP_DETAIL_DELETE",
                actionName: request.Id.ToString(),
                details: $"ApprovalStepDetail with Id {request.Id} deleted successfully.",
                module: "ApprovalStepDetail");
            await _imediator.Publish(domainEvent, cancellationToken);

            return result == true ? result : throw new ExceptionRules("Approval Step Detail deletion failed.");
        }
    }
}