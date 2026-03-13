namespace FinanceManagement.Application.EWaybillHeader.Dto
{
    public sealed class EWaybillHeaderLookupDto
    {
        public int Id { get; set; }
        public string? EWBNumber { get; set; }
        public string? InvoiceNo { get; set; }
        public string? EwbStatus { get; set; }
    }
}
