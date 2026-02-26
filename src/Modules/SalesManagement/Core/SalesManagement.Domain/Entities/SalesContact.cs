using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class SalesContact : BaseEntity
    {
        public string ContactName { get; set; } = null!;
        public string MobileNumber { get; set; } = null!;
        public int ContactTypeId { get; set; }
        public int? PartyId { get; set; }    // cross-module FK (PartyManagement) — no navigation property
        public string? Email { get; set; }
        public string? Remarks { get; set; }

        // Same-module navigation property
        public MiscMaster? ContactType { get; set; }
    }
}
