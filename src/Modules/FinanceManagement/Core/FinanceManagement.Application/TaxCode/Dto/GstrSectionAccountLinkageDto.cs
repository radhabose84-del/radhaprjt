namespace FinanceManagement.Application.TaxCode.Dto
{
    // The GSTR-1/GSTR-3B Account-Range Mapping grid row: section (report+code+name) + account range +
    // Derived vs Expected with the tolerance verdict.
    public class GstrSectionAccountLinkageDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }                   // via parent section

        public int SectionMasterId { get; set; }
        public int ReportTypeId { get; set; }
        public string? ReportType { get; set; }              // REPORT column (GSTR-1 / GSTR-3B)
        public string? SectionCode { get; set; }
        public string? SectionName { get; set; }             // SECTION column = "{SectionCode} — {SectionName}"

        public string? AccountRangeFrom { get; set; }        // ACCOUNT RANGE
        public string? AccountRangeTo { get; set; }

        public decimal? DerivedValue { get; set; }           // DERIVED
        public decimal ExpectedValue { get; set; }           // EXPECTED
        public decimal TolerancePercent { get; set; }

        // TOLERANCE verdict — computed: |Derived - Expected| / Expected * 100 <= TolerancePercent.
        public bool WithinTolerance { get; set; }
        public string? ToleranceStatus { get; set; }         // "Within ±1%" / "Out of tolerance"

        public bool IsActive { get; set; }

        public int CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string? ModifiedByName { get; set; }
    }
}
