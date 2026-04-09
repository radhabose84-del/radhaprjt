using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class AgentCommissionPaymentTerm : BaseEntity
    {
        public int AgentCommissionConfigId { get; set; }
        public int PaymentTermId { get; set; }

        // Navigation properties (same-module only)
        public AgentCommissionConfig? AgentCommissionConfig { get; set; }
        // PaymentTermId is cross-module (PurchaseManagement) — no navigation property, resolved via IPaymentTermLookup
    }
}
