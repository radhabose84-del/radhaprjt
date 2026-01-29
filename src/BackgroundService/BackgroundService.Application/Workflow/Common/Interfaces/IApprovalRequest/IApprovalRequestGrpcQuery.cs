using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.Application.Dto;

using BackgroundService.Domain.Entities.Workflow;

namespace BackgroundService.Application.Workflow.Common.Interfaces.IApprovalRequest
{
    public interface IApprovalRequestGrpcQuery
    {
        Task<List<ApprovalRequest>> GetApprovalRequestByWorkFlowTypeAsync(string WorkFlowType);

        Task<List<ApproverListItemDto>> GetApproverListByWorkFlowTypeAsync(string WorkFlowType, IEnumerable<int> ModuleTransactionIds);
       // Task<List<ApprovalRequest>> GetApproverListByWorkFlowTypeAsync(string WorkFlowType, IEnumerable<int> ModuleTransactionIds);
        Task<List<dynamic>> ApprovalRequestLineStatusByWorkFlowType(string WorkFlowType, IEnumerable<int> ModuleTransactionIds, int UserId);
        Task<List<dynamic>> ApprovalRequestHeaderStatusByWorkFlowType(string WorkFlowType, IEnumerable<int> ModuleTransactionIds, int UserId);
        Task<bool> IsApproveWorkflowConfigure(int MenuId,int UnitId, int DepartmentId);
    }
}