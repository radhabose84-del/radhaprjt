namespace FinanceManagement.Application.EInvoiceHeader.Dto
{
    public class NicIrnResultDto
    {
        public bool IsSuccess { get; set; }
        public string? Irn { get; set; }
        public string? AckNo { get; set; }
        public DateTimeOffset? AckDate { get; set; }
        public string? SignedInvoice { get; set; }
        public string? SignedQRCode { get; set; }
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
