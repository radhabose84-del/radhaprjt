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

        // Reverse navigation (LotMaster)
        public ICollection<LotMaster>? LotMastersAsLotType { get; set; }
        public ICollection<LotMaster>? LotMastersAsStatus { get; set; }

        // Reverse navigation (Production)
        public ICollection<ProductionPackEntry>? ProductionPackEntriesAsQualityStatus { get; set; }

        // Reverse navigation (PackType — PackMaterial)
        public ICollection<PackType>? PackTypesAsPackMaterial { get; set; }
    }
}
