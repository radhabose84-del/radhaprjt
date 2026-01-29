using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.Domain.Entities.Workflow;

namespace BackgroundService.Application.Workflow.Common.Interfaces.IApprovalStepDetail
{
    public interface IApprovalStepDetailCommand
    {
        Task<int> CreateAsync(ApprovalStepDetail approvalStepDetail);     
        Task<bool> UpdateAsync(ApprovalStepDetail approvalStepDetail);
        Task<bool> DeleteAsync(int id,ApprovalStepDetail approvalStepDetail); 
    }
}