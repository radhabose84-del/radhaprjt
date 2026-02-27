using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class MiscTypeMaster : BaseEntity
    {
        public string? MiscTypeCode { get; set; }
        public string? Description { get; set; }

        public ICollection<MiscMaster>? MiscMasters { get; set; }
    }
}
