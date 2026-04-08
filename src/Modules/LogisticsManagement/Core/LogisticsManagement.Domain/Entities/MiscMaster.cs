using LogisticsManagement.Domain.Common;

namespace LogisticsManagement.Domain.Entities
{
    public class MiscMaster : BaseEntity
    {
        public int MiscTypeId { get; set; }
        public string? Code { get; set; }
        public string? Description { get; set; }
        public int SortOrder { get; set; }

        public MiscTypeMaster? MiscTypeMaster { get; set; }

        // Reverse navigation (FreightMaster)
        public ICollection<FreightMaster>? FreightMastersAsFreightMode { get; set; }
        public ICollection<FreightMaster>? FreightMastersAsRateMethod { get; set; }
    }
}
