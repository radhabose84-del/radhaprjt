namespace FinanceManagement.Application.GlAccountMaster.Dto
{
    // US-GL02-10 (AC4) — one row per account code that exists in exactly one company of the entity group.
    public class CoaConsistencyReportItemDto
    {
        public string? AccountCode { get; set; }
        public string? AccountName { get; set; }
        public int CompanyId { get; set; }
        public string? CompanyName { get; set; }

        // e.g. "in Processing only" — the human-readable single-entity flag (AC4).
        public string? Flag { get; set; }
    }
}
