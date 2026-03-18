namespace FinanceManagement.Application.EInvoiceHeader.Dto
{
    public class NicCancelEwbResultDto
    {
        public bool IsSuccess { get; set; }
        public long? EwbNo { get; set; }
        public string? CancelDate { get; set; }
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
