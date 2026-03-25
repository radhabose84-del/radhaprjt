using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class AgentCommissionConfig : BaseEntity
    {
        public int AgentId { get; set; }                    // Cross-module FK → PartyManagement (Party Master)
        public int SalesSegmentId { get; set; }             // Same-module FK → SalesSegment
        public int CommissionTypeId { get; set; }           // Same-module FK → MiscMaster
        public int? CommissionBasisId { get; set; }         // Same-module FK → MiscMaster, optional
        public int? ApplicableLevelId { get; set; }         // Same-module FK → MiscMaster, optional
        public decimal CommissionPercentage { get; set; }
        public int? CurrencyId { get; set; }                // Cross-module FK → UserManagement (Currency), optional
        public DateTimeOffset ValidityFrom { get; set; }
        public DateTimeOffset ValidityTo { get; set; }

        // Navigation Properties (Same-Module FKs only)
        public SalesSegment? SalesSegment { get; set; }
        public MiscMaster? MiscMaster { get; set; }
        public MiscMaster? CommissionBasis { get; set; }
        public MiscMaster? ApplicableLevel { get; set; }
    }
}
