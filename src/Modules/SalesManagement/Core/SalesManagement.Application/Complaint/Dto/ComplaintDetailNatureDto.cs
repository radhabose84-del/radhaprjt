namespace SalesManagement.Application.Complaint.Dto
{
    public class ComplaintDetailNatureDto
    {
        public int Id { get; set; }
        public int ComplaintDetailId { get; set; }
        public int NatureOfComplaintId { get; set; }
        public string? NatureOfComplaintName { get; set; }
    }
}
