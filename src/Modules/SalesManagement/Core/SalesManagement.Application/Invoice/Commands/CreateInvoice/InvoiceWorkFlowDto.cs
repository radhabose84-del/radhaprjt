namespace SalesManagement.Application.Invoice.Commands.CreateInvoice
{
    public class InvoiceWorkFlowDto
    {
        public int Id { get; set; }
        public string? InvoiceNo { get; set; }
        public int? StatusId { get; set; }
        public string? StatusName { get; set; }
        public int? UnitId { get; set; }
    }
}
