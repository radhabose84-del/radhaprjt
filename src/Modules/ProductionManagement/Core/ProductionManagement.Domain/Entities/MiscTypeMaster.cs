using ProductionManagement.Domain.Common;

namespace ProductionManagement.Domain.Entities
{
    public class MiscTypeMaster : BaseEntity
    {
        public string? MiscTypeCode { get; set; }
        public string? Description { get; set; }

        public ICollection<MiscMaster>? MiscMasters { get; set; }
    }
}
