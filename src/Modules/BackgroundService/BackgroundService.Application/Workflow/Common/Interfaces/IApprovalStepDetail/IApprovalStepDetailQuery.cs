using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.Domain.Entities.Workflow;

namespace BackgroundService.Application.Workflow.Common.Interfaces.IApprovalStepDetail
{
    public interface IApprovalStepDetailQuery
    {
        Task<(List<ApprovalStepDetail>, int)> GetAllApprovalStepDetailAsync(int PageNumber, int PageSize, string? SearchTerm);
        Task<bool> AlreadyExistsAsync(int WorkFlowTypeId, int TargetTypeId, int ApprovalStepId, int ApprovalTypeId, int? id = null);
        Task<bool> NotFoundAsync(int id);
        Task<ApprovalStepDetail> GetByIdAsync(int id);
        Task<List<ApprovalStepDetail>> GetApprovalStepDetailAutoComplete(string searchPattern);
    }
}