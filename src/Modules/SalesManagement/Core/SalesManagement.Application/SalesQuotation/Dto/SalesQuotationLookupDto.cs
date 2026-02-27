namespace SalesManagement.Application.SalesQuotation.Dto
{
    public sealed class SalesQuotationLookupDto
    {
        public int Id { get; set; }
        public string? CustomerName { get; set; }
        public DateOnly QuotationDate { get; set; }
        public decimal GrandTotal { get; set; }
        public int TotalItems { get; set; }
    }
}
