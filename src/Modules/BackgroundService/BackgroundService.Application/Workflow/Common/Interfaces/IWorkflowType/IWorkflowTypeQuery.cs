using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.Domain.Entities.Workflow;

namespace BackgroundService.Application.Workflow.Common.Interfaces.IWorkflowType
{
    public interface IWorkflowTypeQuery
    {
        Task<(List<WorkflowType>, int)> GetAllWorkflowTypeAsync(int PageNumber, int PageSize, string? SearchTerm);
        Task<List<WorkflowType>> GetWorkflowTypeAutoComplete(string searchPattern);
        Task<bool> AlreadyExistsAsync(int MenuId, int ModuleId, int? id = null);
        Task<bool> NotFoundAsync(int id);
        Task<WorkflowType> GetWorkflowByName(int MenuId);
    }
}