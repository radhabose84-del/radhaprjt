using AutoMapper;
using BackgroundService.Application.Notification.Exceptions;
using BackgroundService.Application.Workflow.Common.Interfaces.IWorkflowType;
using BackgroundService.Domain.Entities.Workflow;
using BackgroundService.Domain.Events;
using MediatR;

namespace BackgroundService.Application.Workflow.WorkflowTypes.Commands.UpdateWorkflowType
{
    public class UpdateWorkflowTypeCommandHandler : IRequestHandler<UpdateWorkflowTypeCommand, bool>
    {
        private readonly IWorkflowTypeCommand _workflowTypeCommand;
        private readonly IMediator _imediator;
        private readonly IMapper _imapper;

        public UpdateWorkflowTypeCommandHandler(IWorkflowTypeCommand workflowTypeCommand, IMediator imediator, IMapper imapper)
        {
            _workflowTypeCommand = workflowTypeCommand;
            _imediator = imediator;
            _imapper = imapper;
        }

        public async Task<bool> Handle(UpdateWorkflowTypeCommand request, CancellationToken cancellationToken)
        {
            var workflowType = _imapper.Map<WorkflowType>(request);
            var result = await _workflowTypeCommand.UpdateAsync(workflowType);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "WORKFLOW_TYPE_UPDATE",
                actionName: request.Id.ToString(),
                details: $"WorkflowType with Id {request.Id} updated successfully.",
                module: "WorkflowType");
            await _imediator.Publish(domainEvent, cancellationToken);

            return result == true ? result : throw new ExceptionRules("Workflow Type update failed.");
        }
    }
}