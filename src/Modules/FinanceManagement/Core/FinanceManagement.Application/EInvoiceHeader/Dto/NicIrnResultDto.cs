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

        // ── e-Waybill fields (populated only when EwbDtls was included in IRN request) ──
        public long? EwbNo { get; set; }
        public string? EwbDate { get; set; }
        public string? EwbValidTill { get; set; }
    }
}
