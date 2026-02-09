using System.Collections.Generic;
using System.Threading.Tasks;
using Contracts.Dtos.Workflow;

namespace Contracts.Interfaces.Lookups.Workflow
{
    public interface IWorkflowLookup
    {
        Task<List<ApprovalRequestStatusDto>> GetAllApprovalRequestStatusAsync(string moduleTypeName);
        Task<List<ApprovalRequestLineStatusDto>> GetApprovalRequestLineStatusAsync(string moduleTypeName, IEnumerable<int> moduleTransactionIds, int userId);
        Task<List<ApproverListDto>> GetApproverListAsync(string moduleTypeName, IEnumerable<int> moduleTransactionIds);
        Task<List<ApprovalRequestLineStatusDto>> GetApprovalRequestLineAsync(string moduleTypeName, IEnumerable<int> moduleTransactionIds, int userId);
        Task<bool> IsApproveWorkflowConfigureAsync(string menuName, int unitId, int departmentId);
    }
}
