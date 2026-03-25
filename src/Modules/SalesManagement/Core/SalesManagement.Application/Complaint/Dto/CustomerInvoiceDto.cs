namespace SalesManagement.Application.Complaint.Dto
{
    public class CustomerInvoiceDto
    {
        public int Id { get; set; }
        public string? InvoiceNo { get; set; }
        public DateOnly InvoiceDate { get; set; }
        public int InvoiceType { get; set; }
        public string? InvoiceTypeName { get; set; }
        public decimal InvoiceAmount { get; set; }
    }
}
