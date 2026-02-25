
using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class BusinessUnit : BaseEntity
    {
        public string BusinessUnitCode { get; set; } = null!;
        public string BusinessUnitName { get; set; } = null!;
        public string? Description { get; set; }

        // Navigation property for reverse relationship
        public ICollection<SalesSegment> SalesSegments { get; set; } = null!;
    }
}
