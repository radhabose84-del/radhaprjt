namespace SalesManagement.Application.ComplaintResolution.Dto
{
    public class ResolutionListDto
    {
        public int? ResolutionId { get; set; }
        public int ComplaintHeaderId { get; set; }
        public string? ComplaintNumber { get; set; }
        public DateOnly? ComplaintDate { get; set; }
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? ProductName { get; set; }
        public string? ResolutionTypeName { get; set; }
        public string? ClosureStatusName { get; set; }
        public int? ResolvedBy { get; set; }
        public string? ResolvedByName { get; set; }
        public DateTimeOffset? ResolvedDate { get; set; }
    }
}
