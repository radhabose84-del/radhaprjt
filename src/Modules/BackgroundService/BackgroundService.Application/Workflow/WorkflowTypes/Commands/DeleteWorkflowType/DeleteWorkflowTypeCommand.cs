using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace BackgroundService.Application.Workflow.WorkflowTypes.Commands.DeleteWorkflowType
{
    public class DeleteWorkflowTypeCommand : IRequest<bool>
    {
        public int Id { get; set; }
    }
}