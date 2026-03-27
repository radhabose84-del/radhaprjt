namespace SalesManagement.Application.Invoice.Queries.GetInvoiceGatePassPending
{
    public class GetInvoiceGatePassPendingDto
    {
        public int Id { get; set; }
        public string? InvoiceNo { get; set; }
        public DateOnly InvoiceDate { get; set; }
        public int PartyId { get; set; }
        public string? PartyName { get; set; }
        public int UnitId { get; set; }
        public string? UnitName { get; set; }
        public string? CreatedByName { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
    }
}
