using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.Domain.Entities.Workflow;

namespace BackgroundService.Application.Workflow.Common.Interfaces.IApprovalRule
{
    public interface IApprovalRuleCommand
    {
        Task<int> CreateAsync(ApprovalRule approvalRule);     
        Task<bool> UpdateAsync(ApprovalRule approvalRule);
        Task<bool> DeleteAsync(int id,ApprovalRule approvalRule); 
    }
}