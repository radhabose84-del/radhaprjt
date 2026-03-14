using ProductionManagement.Domain.Common;

namespace ProductionManagement.Domain.Entities
{
    public class CountMaster : BaseEntity
    {
        public string? CountCode { get; set; }
        public decimal CountValue { get; set; }
        public string? ShortName { get; set; }
        public int? CountCategoryId { get; set; }
        public int CountTypeId { get; set; }
        public string? CountDescription { get; set; }
        public int UOMId { get; set; }

        // Navigation properties (same-module reverse FK to MiscMaster)
        public MiscMaster? CountType { get; set; }
        public MiscMaster? CountCategory { get; set; }
    }
}
