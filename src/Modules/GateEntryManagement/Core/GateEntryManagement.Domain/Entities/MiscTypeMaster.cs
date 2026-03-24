using GateEntryManagement.Domain.Common;

namespace GateEntryManagement.Domain.Entities
{
    public class MiscTypeMaster : BaseEntity
    {
        public string? MiscTypeCode { get; set; }
        public string? Description { get; set; }

        public ICollection<MiscMaster>? MiscMasters { get; set; }
    }
}
