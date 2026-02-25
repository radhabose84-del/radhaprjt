using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class MiscTypeMaster : BaseEntity
    {
        public string MiscTypeCode { get; set; } = null!;
        public string Description { get; set; } = null!;

        public ICollection<MiscMaster> MiscMasters { get; set; } = null!;
    }
}
