using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.Application.Workflow.ApprovalRequests.Commands.ApproveApprovalRequest;
using BackgroundService.Application.Workflow.ApprovalRequests.Queries.GetApprovalRequestById;
using BackgroundService.Domain.Entities.Workflow;

namespace BackgroundService.Application.Workflow.Common.Interfaces.IApprovalRequest
{
    public interface IApprovalRequestQuery
    {
        Task<(List<dynamic> Lines, dynamic Header)> GetApprovalRequestById(int Id, int ModuleTransactionId);
        Task<dynamic> HeaderLevelApprovalStatus(int Id, int ModuleTransactionId);
        Task<bool> IsLineLevelApproval(int ApprovalRequestId);
        Task<List<ApprovalRequest>> GetApprovedHistory(string WorkflowType, int ModuleTransactionId);
        Task<List<ApprovalRequestLineIdDto>> GetApprovalRequestLinesAsync(
            int approvalRequestHeaderId,
            CancellationToken cancellationToken = default);

        // Task<List<ApprovalRequestWithLinesDto>> GetByModuleAsync(int moduleTransactionId, int workflowTypeId); 

          Task<List<ApprovalRequestWithLinesDto>> GetByModuleAsync(int moduleTransactionId, int workflowTypeId); 
      

        


      
        
    }
}