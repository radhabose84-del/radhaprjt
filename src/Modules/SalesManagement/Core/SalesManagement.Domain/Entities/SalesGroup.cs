using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class SalesGroup : BaseEntity
    {
        public string SalesGroupName { get; set; } = null!;
        public int SalesOfficeId { get; set; }
        public string ResponsibleManager { get; set; } = null!;
        public int? ProductCategoryId { get; set; }
        public string RegionTerritory { get; set; } = null!;

        // Navigation properties
        public SalesOffice SalesOffice { get; set; } = null!;
        public ICollection<OfficerSalesGroup> OfficerSalesGroups { get; set; } = null!;
    }
}
