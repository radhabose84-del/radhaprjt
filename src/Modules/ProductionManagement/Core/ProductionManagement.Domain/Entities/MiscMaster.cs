using ProductionManagement.Domain.Common;

namespace ProductionManagement.Domain.Entities
{
    public class MiscMaster : BaseEntity
    {
        public int MiscTypeId { get; set; }
        public string? Code { get; set; }
        public string? Description { get; set; }
        public int SortOrder { get; set; }

        public MiscTypeMaster? MiscTypeMaster { get; set; }

        public ICollection<CountMaster>? CountMastersAsCountType { get; set; }
        public ICollection<CountMaster>? CountMastersAsCountCategory { get; set; }
    }
}
