using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class ComplaintQCReviewAssignment : BaseEntity
    {
        // Same-module FK → Sales.ComplaintQCReview
        public int ComplaintQCReviewId { get; set; }
        public ComplaintQCReview? ComplaintQCReview { get; set; }

        // Same-module FK → Sales.MiscMaster (QCAssignmentRole: Production/FM, Maintenance, QC Lab, Others)
        public int RoleId { get; set; }
        public MiscMaster? Role { get; set; }

        // Cross-module FK → UserManagement (Employee)
        public int ResponsiblePersonId { get; set; }

        // RCA submission required flag
        public bool IsMandatory { get; set; }

        // Same-module FK → Sales.MiscMaster (QCAssignmentStatus: Pending/Submitted)
        public int AssignmentStatusId { get; set; }
        public MiscMaster? AssignmentStatus { get; set; }
    }
}
