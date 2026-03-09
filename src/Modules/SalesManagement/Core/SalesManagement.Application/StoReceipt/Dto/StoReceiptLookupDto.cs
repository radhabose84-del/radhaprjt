namespace SalesManagement.Application.StoReceipt.Dto
{
    public sealed class StoReceiptLookupDto
    {
        public int Id { get; set; }
        public string? StoReceiptNumber { get; set; }
        public DateOnly StoReceiptDate { get; set; }
        public string? DeliveryNumber { get; set; }
    }
}
