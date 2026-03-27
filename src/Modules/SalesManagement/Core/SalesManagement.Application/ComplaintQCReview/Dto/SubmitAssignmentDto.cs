namespace SalesManagement.Application.ComplaintQCReview.Dto
{
    public class SubmitAssignmentDto
    {
        public int RoleId { get; set; }
        public int ResponsiblePersonId { get; set; }
        public bool IsMandatory { get; set; }
    }
}
