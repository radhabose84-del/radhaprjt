namespace SalesManagement.Application.OfficerAgent.Dto
{
    public class OfficerAgentGroupedDto
    {
        public int MarketingOfficerId { get; set; }
        public string? EmployeeNo { get; set; }
        public string? OfficerName { get; set; }
        public string? Designation { get; set; }
        public string? MobileNo { get; set; }
        public string? Email { get; set; }
        public string? Unit { get; set; }
        public string? Department { get; set; }
        public int SalesOfficeId { get; set; }
        public string? SalesOfficeName { get; set; }

        public List<OfficerAgentItemDto> Agents { get; set; } = new();
    }
}
