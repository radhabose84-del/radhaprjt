namespace SalesManagement.Application.ProformaInvoice.Dto
{
    public sealed class ProformaInvoiceLookupDto
    {
        public int Id { get; set; }
        public string? ProformaNumber { get; set; }
        public DateOnly ProformaDate { get; set; }
        public decimal ProformaAmount { get; set; }
    }
}
