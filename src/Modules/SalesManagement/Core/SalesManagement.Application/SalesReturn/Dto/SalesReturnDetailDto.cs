namespace SalesManagement.Application.SalesReturn.Dto
{
    public class SalesReturnInvoiceResponseDto
    {
        public int InvoiceHeaderId { get; set; }
        public string? InvoiceNo { get; set; }
        public DateOnly? InvoiceDate { get; set; }
        public List<SalesReturnDetailDto>? Items { get; set; }
    }

    public class SalesReturnDetailDto
    {
        public int Id { get; set; }
        public int SalesReturnHeaderId { get; set; }
        public int InvoiceHeaderId { get; set; }
        public string? InvoiceNo { get; set; }
        public DateOnly? InvoiceDate { get; set; }
        public int InvoiceDetailId { get; set; }
        public int ItemId { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public int? LotId { get; set; }
        public string? LotCode { get; set; }
        public int StartPackNo { get; set; }
        public int EndPackNo { get; set; }
        public decimal ReturnQty { get; set; }
        public int? PackTypeId { get; set; }
        public int BagStatusId { get; set; }
        public string? BagStatusName { get; set; }
    }
}
