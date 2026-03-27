namespace SalesManagement.Application.ComplaintQCReview.Dto
{
    public class ComplaintQCReviewAssignmentDto
    {
        public int Id { get; set; }
        public int ComplaintQCReviewId { get; set; }
        public int RoleId { get; set; }
        public string? RoleName { get; set; }
        public int ResponsiblePersonId { get; set; }
        public string? ResponsiblePersonName { get; set; }
        public bool IsMandatory { get; set; }
        public int AssignmentStatusId { get; set; }
        public string? AssignmentStatusName { get; set; }
    }
}
