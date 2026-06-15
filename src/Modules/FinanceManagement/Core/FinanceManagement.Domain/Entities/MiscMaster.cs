using FinanceManagement.Domain.Common;

namespace FinanceManagement.Domain.Entities
{
    public class MiscMaster : BaseEntity
    {
        public int MiscTypeId { get; set; }
        public string? Code { get; set; }
        public string? Description { get; set; }
        public int SortOrder { get; set; }

        // Same-module FK navigation
        public MiscTypeMaster? MiscTypeMaster { get; set; }
    }
}
