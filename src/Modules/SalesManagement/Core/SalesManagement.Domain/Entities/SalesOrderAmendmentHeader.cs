using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class SalesOrderAmendmentHeader : BaseEntity
    {
        public int SalesOrderHeaderId { get; set; }
        public int UnitId { get; set; }                  // Cross-module FK (UserManagement)
        public string? AmendmentNo { get; set; }
        public int RevisionNumber { get; set; }
        public DateOnly AmendmentDate { get; set; }
        public string? Reason { get; set; }

        // Approval Status (same-module FK to MiscMaster)
        public int? StatusId { get; set; }

        // Who approved/rejected
        public int? ApprovedBy { get; set; }
        public DateTimeOffset? ApprovedDate { get; set; }

        // Navigation Properties
        public SalesOrderHeader? SalesOrderHeader { get; set; }
        public MiscMaster? StatusMisc { get; set; }

        // Child collection
        public ICollection<SalesOrderAmendmentDetail>? SalesOrderAmendmentDetails { get; set; }
    }
}
