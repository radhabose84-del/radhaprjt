using AutoMapper;
using BackgroundService.Application.Notification.Exceptions;
using BackgroundService.Application.Workflow.Common.Interfaces.IWorkflowType;
using BackgroundService.Domain.Entities.Workflow;
using BackgroundService.Domain.Events;
using MediatR;

namespace BackgroundService.Application.Workflow.WorkflowTypes.Commands.CreateWorkflowType
{
    public class CreateWorkflowTypeCommandHandler : IRequestHandler<CreateWorkflowTypeCommand, List<int>>
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

        public async Task<List<int>> Handle(CreateWorkflowTypeCommand request, CancellationToken cancellationToken)
        {
            var transactionTypeIds = request.TransactionTypeIds ?? new List<int>();

            var workflowTypes = new List<WorkflowType>();

            if (transactionTypeIds.Count == 0)
            {
                // No transaction types — create single row with TransactionTypeId = null
                var entity = _imapper.Map<WorkflowType>(request);
                entity.TransactionTypeId = null;
                workflowTypes.Add(entity);
            }
            else
            {
                // Create one row per TransactionTypeId
                foreach (var txnTypeId in transactionTypeIds)
                {
                    var entity = _imapper.Map<WorkflowType>(request);
                    entity.TransactionTypeId = txnTypeId;
                    workflowTypes.Add(entity);
                }
            }

            var newIds = await _workflowTypeCommand.CreateBulkAsync(workflowTypes);

            if (newIds.Count == 0)
                throw new ExceptionRules("Workflow Type Creation Failed.");

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "WORKFLOW_TYPE_CREATE",
                actionName: request.MenuId.ToString(),
                details: $"WorkflowType created with MenuId {request.MenuId}, ModuleId {request.ModuleId}, {newIds.Count} record(s).",
                module: "WorkflowType");
            await _imediator.Publish(domainEvent, cancellationToken);

            return newIds;
        }
    }
}
