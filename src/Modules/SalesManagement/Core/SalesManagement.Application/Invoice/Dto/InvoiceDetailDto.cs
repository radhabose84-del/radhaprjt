namespace SalesManagement.Application.Invoice.Dto
{
    public class InvoiceDetailDto
    {
        public int Id { get; set; }
        public int InvoiceHeaderId { get; set; }
        public int ItemSno { get; set; }
        public int ItemId { get; set; }
        public string? ItemName { get; set; }
        public string? HsnCode { get; set; }
        public decimal GstPercentage { get; set; }
        public int? LotId { get; set; }
        public string? LotNo { get; set; }
        public int NoOfBags { get; set; }
        public decimal Quantity { get; set; }
        public decimal RatePerKg { get; set; }
        public decimal Discount { get; set; }
        public decimal TaxableAmount { get; set; }
        public decimal CgstPercentage { get; set; }
        public decimal SgstPercentage { get; set; }
        public decimal IgstPercentage { get; set; }
        public decimal CGST { get; set; }
        public decimal SGST { get; set; }
        public decimal IGST { get; set; }
        public decimal TaxAmount { get; set; }
        public int? PackTypeId { get; set; }
        public string? PackTypeName { get; set; }
        public int? UOMId { get; set; }
        public string? UOMName { get; set; }
        public decimal Charity { get; set; }
        public decimal HandlingCharges { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
