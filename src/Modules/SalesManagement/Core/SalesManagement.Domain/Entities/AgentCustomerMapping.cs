using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class AgentCustomerMapping : BaseEntity
    {
        public int CustomerId { get; set; }         // Cross-module FK → PartyManagement (Party Type = Customer)
        public int AgentId { get; set; }            // Cross-module FK → PartyManagement (Party Type = Agent)
        public int? SubAgentId { get; set; }        // Cross-module FK → PartyManagement (Party Type = Agent), optional
        public int SalesGroupId { get; set; }         // Same-module FK → Sales.SalesGroup
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public bool IsDefaultAgent { get; set; }
        public string? Remarks { get; set; }

        // Navigation property (same-module FK)
        public SalesGroup? SalesGroup { get; set; }
    }
}
