namespace FinanceManagement.Application.JournalMaster.Dto
{
    // Print/PDF model for a posted journal voucher (US-GL01-18). Everything a voucher print needs:
    // company header, voucher meta, lines, totals, maker/checker, and a deterministic content fingerprint.
    public sealed class JournalPrintDto
    {
        // Company header
        public int CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public string? CompanyLegalName { get; set; }
        public string? CompanyGstin { get; set; }
        public int? UnitId { get; set; }
        public string? UnitName { get; set; }

        // Voucher meta
        public int Id { get; set; }
        public string? VoucherNo { get; set; }
        public DateOnly VoucherDate { get; set; }
        public DateOnly? PostingDate { get; set; }
        public string? StatusName { get; set; }
        public string? Narration { get; set; }
        public decimal TotalDr { get; set; }
        public decimal TotalCr { get; set; }

        // Maker / checker
        public string? PreparedByName { get; set; }
        public DateTimeOffset? PreparedAt { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTimeOffset? ApprovedAt { get; set; }

        // Seal / verification — deterministic: re-generation always yields the same fingerprint.
        public string? Fingerprint { get; set; }
        public string? VerifyUrl { get; set; }

        public List<JournalPrintLineDto> Lines { get; set; } = new();
    }

    public sealed class JournalPrintLineDto
    {
        public int LineNo { get; set; }
        public int GlAccountId { get; set; }
        public string? AccountCode { get; set; }
        public string? AccountName { get; set; }
        public int? CostCentreId { get; set; }
        public string? CostCentreName { get; set; }
        public decimal DrAmount { get; set; }
        public decimal CrAmount { get; set; }
        public string? LineNarration { get; set; }
    }
}
