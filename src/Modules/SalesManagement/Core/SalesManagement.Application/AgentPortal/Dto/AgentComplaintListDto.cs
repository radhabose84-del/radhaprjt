namespace SalesManagement.Application.AgentPortal.Dto
{
    public class AgentComplaintListDto
    {
        public int Id { get; set; }
        public string? ComplaintNumber { get; set; }
        public DateOnly ComplaintDate { get; set; }
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? StatusName { get; set; }
        public string? Remarks { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
    }
}
