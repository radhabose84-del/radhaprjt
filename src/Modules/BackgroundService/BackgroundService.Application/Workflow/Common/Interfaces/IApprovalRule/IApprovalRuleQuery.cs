using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.Domain.Entities.Workflow;

namespace BackgroundService.Application.Workflow.Common.Interfaces.IApprovalRule
{
    public interface IApprovalRuleQuery
    {
        Task<(List<ApprovalRule>, int)> GetAllApprovalRuleAsync(int PageNumber, int PageSize, string? SearchTerm);
        Task<bool> AlreadyExistsAsync(string ConditionKey, string Operator, string Value, string Action, int UnitId, int WorkFlowTypeId, int? id = null);
        Task<bool> NotFoundAsync(int id);
        Task<ApprovalRule> GetByIdAsync(int id);
    }
}