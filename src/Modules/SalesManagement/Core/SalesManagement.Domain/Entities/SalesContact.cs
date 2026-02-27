using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class SalesContact : BaseEntity
    {
        public string? ContactName { get; set; }
        public string? MobileNumber { get; set; }
        public int ContactTypeId { get; set; }
        public int? PartyId { get; set; }    // cross-module FK (PartyManagement) — no navigation property
        public string? Email { get; set; }
        public string? Remarks { get; set; }

        // Same-module navigation property
        public MiscMaster? ContactType { get; set; }
    }
}
