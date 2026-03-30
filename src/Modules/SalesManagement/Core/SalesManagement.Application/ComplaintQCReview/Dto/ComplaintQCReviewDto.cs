namespace SalesManagement.Application.ComplaintQCReview.Dto
{
    public class ComplaintQCReviewDto
    {
        public int Id { get; set; }
        public int ComplaintHeaderId { get; set; }
        public string? ComplaintNumber { get; set; }
        public DateOnly? ComplaintDate { get; set; }
        public string? CustomerName { get; set; }
        public string? ItemName { get; set; }

        // Physical Verification
        public int PhysicalVerificationId { get; set; }
        public string? PhysicalVerificationName { get; set; }

        // Complaint Status
        public int? ComplaintStatusId { get; set; }
        public string? ComplaintStatusName { get; set; }

        // Severity
        public int? SeverityId { get; set; }
        public string? SeverityName { get; set; }

        // Compensation Structure
        public int? CompensationStructureId { get; set; }
        public string? CompensationStructureName { get; set; }

        // Lab Verification
        public bool LabVerificationRequired { get; set; }
        public int? LabResponsiblePersonId { get; set; }
        public string? LabResponsiblePersonName { get; set; }

        // Resolution
        public DateOnly? ExpectedResolutionDate { get; set; }
        public string? Comments { get; set; }

        // Audit
        public int? ReviewedBy { get; set; }
        public string? ReviewedByName { get; set; }
        public DateTimeOffset? ReviewedDate { get; set; }
        public DateTimeOffset? DecisionTimestamp { get; set; }

        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        // Derived Review Status (based on assignment feedbacks)
        public string? ReviewStatus { get; set; }

        // Assignments
        public List<ComplaintQCReviewAssignmentDto>? Assignments { get; set; }
    }
}
