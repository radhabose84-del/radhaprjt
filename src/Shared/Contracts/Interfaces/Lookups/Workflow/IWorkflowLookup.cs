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

        /// <summary>
        /// Returns the workflow-resolved approver UserId for a transaction, looking up
        /// AppData.ApprovalRequest by (WorkflowType, ModuleTransactionId, UnitId, ApprovalRuleId).
        /// Use ApprovalRuleId to select rule-conditional steps — e.g., ApprovalRuleId = 1
        /// returns the MD-level approver registered when MdDiscount was enabled.
        /// Returns null when no row matches (the rule-conditional step was not registered).
        /// </summary>
        Task<int?> GetApproverUserIdByRuleAsync(
            string workflowType,
            int moduleTransactionId,
            int unitId,
            int approvalRuleId,
            CancellationToken cancellationToken = default);
    }
}
