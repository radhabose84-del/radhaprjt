namespace FinanceManagement.Application.GlAccountMaster.Dto
{
    // US-GL02-10 (AC5) — entry for the mandatory company selector on account screens.
    public class CompanyOptionDto
    {
        public int CompanyId { get; set; }
        public string? CompanyName { get; set; }
    }
}
