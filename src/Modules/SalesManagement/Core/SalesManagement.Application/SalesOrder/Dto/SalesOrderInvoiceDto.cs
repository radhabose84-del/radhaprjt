namespace SalesManagement.Application.SalesOrder.Dto
{
    public class SalesOrderInvoiceDto
    {
        public int InvoiceId { get; set; }
        public string? InvoiceNo { get; set; }
        public DateOnly InvoiceDate { get; set; }
        public decimal InvoiceAmount { get; set; }
        public int PartyId { get; set; }
        public string? PartyName { get; set; }
        public string? StatusName { get; set; }
        public int? DispatchAdviceId { get; set; }
        public string? DispatchNo { get; set; }
        public DateOnly? DispatchDate { get; set; }
    }
}
