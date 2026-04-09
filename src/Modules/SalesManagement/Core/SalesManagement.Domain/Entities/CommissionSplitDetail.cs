using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class CommissionSplitDetail : BaseEntity
    {
        public int CommissionSplitId { get; set; }
        public int RoleId { get; set; }
        public int ShareTypeId { get; set; }
        public decimal ShareValue { get; set; }

        // Navigation properties
        public CommissionSplit? CommissionSplit { get; set; }
        public MiscMaster? Role { get; set; }
        public MiscMaster? ShareType { get; set; }
    }
}
