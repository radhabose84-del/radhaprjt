#nullable disable

using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class BusinessUnit : BaseEntity
    {
        public string BusinessUnitCode { get; set; }
        public string BusinessUnitName { get; set; }
        public string Description { get; set; }

        // Navigation property for reverse relationship
        public ICollection<SalesSegment> SalesSegments { get; set; }
    }
}
