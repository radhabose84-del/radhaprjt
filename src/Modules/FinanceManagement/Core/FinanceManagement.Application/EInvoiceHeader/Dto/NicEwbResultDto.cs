namespace FinanceManagement.Application.EInvoiceHeader.Dto
{
    public class NicEwbResultDto
    {
        public bool IsSuccess { get; set; }
        public long? EwbNo { get; set; }
        public string? EwbDate { get; set; }
        public string? EwbValidTill { get; set; }
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
