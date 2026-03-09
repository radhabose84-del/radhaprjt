namespace SalesManagement.Application.Invoice.Dto
{
    public sealed class InvoiceLookupDto
    {
        public int Id { get; set; }
        public string? InvoiceNo { get; set; }
        public DateOnly InvoiceDate { get; set; }
        public string? PartyName { get; set; }
    }
}
