namespace SalesManagement.Application.OfficerAgent.Dto
{
    public class OfficerAgentDto
    {
        public int Id { get; set; }

        public int AgentId { get; set; }
        public string? AgentName { get; set; }          // Populated via IPartyLookup

        public int MarketingOfficerId { get; set; }
        public string? OfficerName { get; set; }        // Populated via SQL JOIN Sales.MarketingOfficer

        public DateOnly ValidityFrom { get; set; }
        public DateOnly ValidityTo { get; set; }

        public bool IsActive { get; set; }

        // Audit fields
        public int CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string? ModifiedByName { get; set; }
        public string? ModifiedIP { get; set; }
    }
}
