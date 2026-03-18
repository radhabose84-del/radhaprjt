namespace FinanceManagement.Application.EInvoiceHeader.Dto
{
    public class NicCancelIrnResultDto
    {
        public bool IsSuccess { get; set; }
        public string? Irn { get; set; }
        public string? CancelDate { get; set; }
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
