namespace Contracts.Dtos.Finance
{
    public class EInvoiceCreationResultDto
    {
        public int EInvoiceHeaderId { get; set; }
        public string? Irn { get; set; }
        public string? AckNo { get; set; }
        public DateTimeOffset? AckDate { get; set; }
        public long? EwbNo { get; set; }
        public string? EwbDate { get; set; }
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
