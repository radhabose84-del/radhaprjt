namespace FinanceManagement.Application.VoucherType.Dto
{
    // Header stat cards on the Voucher Type Configuration page.
    public class VoucherTypeSummaryDto
    {
        public int TotalCount { get; set; }
        public int ActiveCount { get; set; }
        public int SystemCount { get; set; }
        public int CustomCount { get; set; }
    }
}
