#nullable disable
using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class SalesGroup : BaseEntity
    {
        public string SalesGroupName { get; set; }
        public int SalesOfficeId { get; set; }
        public string ResponsibleManager { get; set; }
        public int? ProductCategoryId { get; set; }
        public string RegionTerritory { get; set; }

        // Navigation property
        public SalesOffice SalesOffice { get; set; }
    }
}
