namespace SalesManagement.Application.Invoice.Dto
{
    public class EInvoiceDetailDto
    {
        public int Id { get; set; }
        public int EInvoiceHeaderId { get; set; }
        public int ItemSno { get; set; }
        public int ItemId { get; set; }
        public string? ItemName { get; set; }
        public string? HsnNo { get; set; }
        public int NoOfBags { get; set; }
        public decimal Qty { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Rate { get; set; }
        public decimal Discount { get; set; }
        public decimal TaxableAmount { get; set; }
        public decimal GstPercentage { get; set; }
        public decimal CGST { get; set; }
        public decimal SGST { get; set; }
        public decimal IGST { get; set; }
        public decimal OtherCharges { get; set; }
        public decimal TotalAmount { get; set; }
        public int? PackTypeId { get; set; }
        public string? UOM { get; set; }
    }
}
