using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BackgroundService.Application.Notification.Exceptions;
using BackgroundService.Application.Workflow.Common.Interfaces.IWorkflowType;
using BackgroundService.Domain.Entities.Workflow;
using MediatR;

namespace BackgroundService.Application.Workflow.WorkflowTypes.Commands.DeleteWorkflowType
{
    public class DeleteWorkflowTypeCommandHandler : IRequestHandler<DeleteWorkflowTypeCommand, bool>
    {
        private readonly IWorkflowTypeCommand _workflowTypeCommand;
        private readonly IMediator _imediator;
        private readonly IMapper _imapper;
        public DeleteWorkflowTypeCommandHandler(IWorkflowTypeCommand workflowTypeCommand, IMediator imediator, IMapper imapper)
        {
            _workflowTypeCommand = workflowTypeCommand;
            _imediator = imediator;
            _imapper = imapper;
        }
        public async Task<bool> Handle(DeleteWorkflowTypeCommand request, CancellationToken cancellationToken)
        {
             var WorkflowType = _imapper.Map<WorkflowType>(request);
            var result = await _workflowTypeCommand.DeleteAsync(request.Id,WorkflowType);
        
            return result == true ? result : throw new ExceptionRules("Workflow Type deletion failed.");
        }
    }
}