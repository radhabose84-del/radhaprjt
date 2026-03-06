using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.Application.Workflow.Common.Interfaces.IWorkflowType;
using BackgroundService.Domain.Entities.Workflow;
using BackgroundService.Infrastructure.Data.Notification;
using Microsoft.EntityFrameworkCore;

namespace BackgroundService.Infrastructure.Repositories.Workflow.WorkflowTypes
{
    public class WorkflowTypeCommandRepository : IWorkflowTypeCommand
    {
        private readonly NotificationDbContext _notificationDbContext;
        public WorkflowTypeCommandRepository(NotificationDbContext notificationDbContext)
        {
            _notificationDbContext = notificationDbContext;
        }
        public async Task<int> CreateAsync(WorkflowType workflowType)
        {
             _notificationDbContext.Entry(workflowType);
            await _notificationDbContext.WorkflowType.AddAsync(workflowType);
            await _notificationDbContext.SaveChangesAsync();

            return workflowType.Id;
        }

        public async Task<bool> DeleteAsync(int id, WorkflowType workflowType)
        {
              var WorkFlowDelete = await _notificationDbContext.WorkflowType.FirstOrDefaultAsync(u => u.Id == id);
            if (WorkFlowDelete != null)
            {
                WorkFlowDelete.IsDeleted = workflowType.IsDeleted;
                return await _notificationDbContext.SaveChangesAsync() >0;
            }
            return false; 
        }

        public async Task<bool> UpdateAsync(WorkflowType workflowType)
        {
            var existingWorkflow = await _notificationDbContext.WorkflowType
            .AsNoTracking().FirstOrDefaultAsync(u => u.Id == workflowType.Id);
            
            if (existingWorkflow != null)
            {
                existingWorkflow.MenuId = workflowType.MenuId;
                existingWorkflow.ModuleId = workflowType.ModuleId;
                existingWorkflow.HasLine = workflowType.HasLine;
                existingWorkflow.IsMultiselect = workflowType.IsMultiselect;
                existingWorkflow.IsActive = workflowType.IsActive;
                _notificationDbContext.WorkflowType.Update(existingWorkflow);

                return await _notificationDbContext.SaveChangesAsync() >0;
            }
            
            return false; 
        }
    }
}