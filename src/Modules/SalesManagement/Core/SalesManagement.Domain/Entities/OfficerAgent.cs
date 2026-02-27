namespace SalesManagement.Domain.Entities
{
    /// <summary>
    /// Assigns an Agent (PartyManagement) to a Marketing Officer within SalesManagement.
    /// Does not extend BaseEntity — no IsDeleted, hard delete only.
    /// Audit fields are populated by ApplicationDbContext.UpdateIpFields().
    /// </summary>
    public class OfficerAgent
    {
        public int Id { get; set; }

        public int AgentId { get; set; }                // Cross-module FK → PartyManagement.Party

        public int MarketingOfficerId { get; set; }     // Same-module FK → Sales.MarketingOfficer

        public DateOnly ValidityFrom { get; set; }

        public DateOnly ValidityTo { get; set; }

        public bool IsActive { get; set; } = true;

        // Audit fields — auto-populated by ApplicationDbContext
        public int CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string? ModifiedByName { get; set; }
        public string? ModifiedIP { get; set; }

        // Navigation property — same-module FK only
        public MarketingOfficer? MarketingOfficer { get; set; }
    }
}
