namespace FinanceManagement.Application.VoucherType.Dto
{
    // Backs the "Number Series & FY Reset" tab — one row per voucher type for the selected fiscal year.
    public class VoucherTypeNumberSeriesDto
    {
        public int VoucherTypeId { get; set; }
        public string? VoucherTypeCode { get; set; }
        public string? VoucherTypeName { get; set; }
        public int NumberPadding { get; set; }
        public bool IsSystem { get; set; }

        public int FinancialYearId { get; set; }
        public string? FinancialYearName { get; set; }

        public int LastUsedNumber { get; set; }      // 0 if no row yet for this FY
        public int NextNumberValue { get; set; }      // LastUsedNumber + 1
        public string? NextNumber { get; set; }       // formatted e.g. "JV/2026-27/0428"
    }
}
