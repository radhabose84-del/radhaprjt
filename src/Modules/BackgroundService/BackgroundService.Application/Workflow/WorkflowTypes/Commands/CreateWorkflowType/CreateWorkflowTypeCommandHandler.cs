using AutoMapper;
using BackgroundService.Application.Notification.Exceptions;
using BackgroundService.Application.Workflow.Common.Interfaces.IWorkflowType;
using BackgroundService.Domain.Entities.Workflow;
using BackgroundService.Domain.Events;
using MediatR;

namespace BackgroundService.Application.Workflow.WorkflowTypes.Commands.CreateWorkflowType
{
    public class CreateWorkflowTypeCommandHandler : IRequestHandler<CreateWorkflowTypeCommand, int>
    {
        private readonly IWorkflowTypeCommand _workflowTypeCommand;
        private readonly IMediator _imediator;
        private readonly IMapper _imapper;

        public CreateWorkflowTypeCommandHandler(IWorkflowTypeCommand workflowTypeCommand, IMediator imediator, IMapper imapper)
        {
            _workflowTypeCommand = workflowTypeCommand;
            _imediator = imediator;
            _imapper = imapper;
        }

        public async Task<int> Handle(CreateWorkflowTypeCommand request, CancellationToken cancellationToken)
        {
            var workflowType = _imapper.Map<WorkflowType>(request);
            var result = await _workflowTypeCommand.CreateAsync(workflowType);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "WORKFLOW_TYPE_CREATE",
                actionName: request.MenuId.ToString(),
                details: $"WorkflowType created with MenuId {request.MenuId} and ModuleId {request.ModuleId}.",
                module: "WorkflowType");
            await _imediator.Publish(domainEvent, cancellationToken);

            return result > 0 ? result : throw new ExceptionRules("Workflow Type Creation Failed.");
        }
    }
}