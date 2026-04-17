using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class AgentCommissionSlab : BaseEntity
    {
        public int AgentCommissionConfigId { get; set; }
        public int SlabOrder { get; set; }
        public int FromDelay { get; set; }
        public int? ToDelay { get; set; }
        public int CommissionTypeId { get; set; }
        public int CommissionBasisId { get; set; }
        public decimal CommissionValue { get; set; }

        // Navigation properties
        public AgentCommissionConfig? AgentCommissionConfig { get; set; }
        public MiscMaster? CommissionType { get; set; }
        public MiscMaster? CommissionBasis { get; set; }

        // Reverse navigation (SalesOrderHeader)
        public ICollection<SalesOrderHeader>? SalesOrderHeaders { get; set; }
    }
}
