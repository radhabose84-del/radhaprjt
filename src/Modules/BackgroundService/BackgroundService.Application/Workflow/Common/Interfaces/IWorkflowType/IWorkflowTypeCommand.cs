using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.Domain.Entities.Workflow;

namespace BackgroundService.Application.Workflow.Common.Interfaces.IWorkflowType
{
    public interface IWorkflowTypeCommand
    {
         Task<int> CreateAsync(WorkflowType workflowType);     
        Task<bool> UpdateAsync(WorkflowType workflowType);
        Task<bool> DeleteAsync(int id,WorkflowType workflowType); 
    }
}