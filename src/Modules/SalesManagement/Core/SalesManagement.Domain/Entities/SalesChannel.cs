using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class SalesChannel : BaseEntity
    {
        public string SalesChannelCode { get; set; } = null!;
        public string SalesChannelName { get; set; } = null!;

        // Navigation property for reverse relationship
        public ICollection<SalesSegment> SalesSegments { get; set; } = null!;
    }
}
