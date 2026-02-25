using Contracts.Dtos.Workflow;

namespace Contracts.Interfaces.External.IWorkflow
{
    public interface IWorkflowGrpcClient
    {
        Task<List<ApprovalRequestStatusDto>> GetAllApprovalRequestStatusAsync(string ModuleTypeName);
        Task<List<ApprovalRequestLineStatusDto>> GetApprovalRequestLineStatusAsync(string ModuleTypeName, IEnumerable<int> ModuleTransactionIds, int UserId);
        Task<List<ApproverListDto>> GetApproverListAsync(string ModuleTypeName, IEnumerable<int> ModuleTransactionIds);
        Task<List<ApprovalRequestLineStatusDto>> GetApprovalRequestLineAsync(string ModuleTypeName, IEnumerable<int> ModuleTransactionIds, int UserId);
        Task<bool> IsApproveWorkflowConfigure(string MenuName,int UnitId, int DepartmentId);
    }
}