#nullable disable
using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class SalesChannel : BaseEntity
    {
        public string SalesChannelCode { get; set; }
        public string SalesChannelName { get; set; }

        // Navigation property for reverse relationship
        public ICollection<SalesSegment> SalesSegments { get; set; }
    }
}
