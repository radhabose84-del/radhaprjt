namespace SalesManagement.Application.Complaint.Dto
{
    public class InvoiceLineDetailDto
    {
        public int InvoiceHeaderId { get; set; }
        public string? InvoiceNo { get; set; }
        public DateOnly InvoiceDate { get; set; }
        public int InvoiceType { get; set; }
        public string? InvoiceTypeName { get; set; }
        public int? LotId { get; set; }
        public string? LotCode { get; set; }
        public int ItemId { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public int NoOfBags { get; set; }
        public decimal Quantity { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
