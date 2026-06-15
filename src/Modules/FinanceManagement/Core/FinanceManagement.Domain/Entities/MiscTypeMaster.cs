using FinanceManagement.Domain.Common;

namespace FinanceManagement.Domain.Entities
{
    public class MiscTypeMaster : BaseEntity
    {
        public string? MiscTypeCode { get; set; }
        public string? Description { get; set; }

        // Same-module child collection
        public ICollection<MiscMaster>? MiscMasters { get; set; }
    }
}
