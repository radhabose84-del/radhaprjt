using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class ComplaintQCReview : BaseEntity
    {
        // Same-module FK → Sales.ComplaintHeader (one review per complaint)
        public int ComplaintHeaderId { get; set; }
        public ComplaintHeader? ComplaintHeader { get; set; }

        // Same-module FK → Sales.MiscMaster (PhysicalVerification: Pending/In Progress/Completed)
        public int PhysicalVerificationId { get; set; }
        public MiscMaster? PhysicalVerification { get; set; }

        // Same-module FK → Sales.MiscMaster (QCComplaintStatus: QC Accepted/QC Rejected)
        public int? ComplaintStatusId { get; set; }
        public MiscMaster? ComplaintStatus { get; set; }

        // Same-module FK → Sales.MiscMaster (ComplaintSeverity: Minor/Major/Critical)
        public int? SeverityId { get; set; }
        public MiscMaster? Severity { get; set; }

        // Same-module FK → Sales.MiscMaster (CompensationStructure: Credit Note/Replacement/Reprocess/No Compensation)
        public int? CompensationStructureId { get; set; }
        public MiscMaster? CompensationStructure { get; set; }

        // Lab Verification
        public bool LabVerificationRequired { get; set; }

        // Cross-module FK → UserManagement (Employee)
        public int? LabResponsiblePersonId { get; set; }

        // Resolution
        public DateOnly? ExpectedResolutionDate { get; set; }
        public string? Comments { get; set; }

        // Audit fields specific to QC Review
        public int? ReviewedBy { get; set; }
        public DateTimeOffset? ReviewedDate { get; set; }
        public DateTimeOffset? DecisionTimestamp { get; set; }

        // Child collection
        public ICollection<ComplaintQCReviewAssignment>? Assignments { get; set; }
    }
}
