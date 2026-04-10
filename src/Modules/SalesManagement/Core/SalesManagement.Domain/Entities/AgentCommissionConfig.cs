using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class AgentCommissionConfig : BaseEntity
    {
        public int AgentId { get; set; }                    // Cross-module FK → PartyManagement (Party Master)
        public int CommissionTypeId { get; set; }           // Same-module FK → MiscMaster (Percentage / Fixed Amount)
        public int CommissionBasisId { get; set; }          // Same-module FK → MiscMaster (Invoice Value / Net Value)
        public int ApplicableLevelId { get; set; }          // Same-module FK → MiscMaster (Header)
        public decimal CommissionPercentage { get; set; }   // "Commission Value" in UI
        public DateTimeOffset ValidityFrom { get; set; }
        public DateTimeOffset? ValidityTo { get; set; }     // Optional (>= From)
        public int TriggerEventId { get; set; }             // Same-module FK → MiscMaster ("Payment")
        public int? SlabTypeId { get; set; }                // Same-module FK → MiscMaster ("Delay Days"), optional
        public int CommissionSplitId { get; set; }          // Same-module FK → CommissionSplit

        // Navigation Properties (Same-Module FKs only)
        public MiscMaster? MiscMaster { get; set; }         // CommissionType
        public MiscMaster? CommissionBasis { get; set; }
        public MiscMaster? ApplicableLevel { get; set; }
        public MiscMaster? TriggerEvent { get; set; }
        public MiscMaster? SlabType { get; set; }
        public CommissionSplit? CommissionSplit { get; set; }

        // Child collections
        public ICollection<AgentCommissionSalesGroup>? AgentCommissionSalesGroups { get; set; }
        public ICollection<AgentCommissionPaymentTerm>? AgentCommissionPaymentTerms { get; set; }
        public ICollection<AgentCommissionSlab>? AgentCommissionSlabs { get; set; }
    }
}
