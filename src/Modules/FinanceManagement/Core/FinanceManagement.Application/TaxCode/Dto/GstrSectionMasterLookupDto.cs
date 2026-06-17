namespace FinanceManagement.Application.TaxCode.Dto
{
    // Lightweight section lookup for the linkage screen's Section dropdown.
    public sealed class GstrSectionMasterLookupDto
    {
        public int Id { get; set; }
        public int ReportTypeId { get; set; }
        public string? ReportType { get; set; }
        public string? SectionCode { get; set; }
        public string? SectionName { get; set; }
    }
}
