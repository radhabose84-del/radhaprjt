using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class CommissionSplit : BaseEntity
    {
        public string? SplitCode { get; set; }
        public string? SplitName { get; set; }

        // Child collection
        public ICollection<CommissionSplitDetail>? CommissionSplitDetails { get; set; }

        // Reverse navigation (AgentCommissionConfig)
        public ICollection<AgentCommissionConfig>? AgentCommissionConfigs { get; set; }
    }
}
