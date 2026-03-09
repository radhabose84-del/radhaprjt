namespace SalesManagement.Application.Invoice.Dto
{
    public class CreateInvoiceDetailDto
    {
        public int ItemSno { get; set; }
        public int ItemId { get; set; }
        public string? HsnCode { get; set; }
        public decimal GstPercentage { get; set; }
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
        public string? UOM { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
