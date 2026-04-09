using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class AgentCommissionSalesGroup : BaseEntity
    {
        public int AgentCommissionConfigId { get; set; }
        public int SalesGroupId { get; set; }

        // Navigation properties
        public AgentCommissionConfig? AgentCommissionConfig { get; set; }
        public SalesGroup? SalesGroup { get; set; }
    }
}
