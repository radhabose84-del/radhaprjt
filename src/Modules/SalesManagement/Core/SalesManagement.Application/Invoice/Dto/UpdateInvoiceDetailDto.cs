namespace SalesManagement.Application.Invoice.Dto
{
    public class UpdateInvoiceDetailDto
    {
        public int Id { get; set; }
        public int ItemSno { get; set; }
        public int ItemId { get; set; }
        public string? HsnCode { get; set; }
        public decimal GstPercentage { get; set; }
        public int? LotId { get; set; }
        public decimal NoOfBags { get; set; }
        public decimal BagWeight { get; set; }
        public decimal NetWeight { get; set; }
        public decimal RatePerKg { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal FreightValue { get; set; }
        public decimal CommissionValue { get; set; }
        public decimal TaxableAmount { get; set; }
        public decimal CgstPercentage { get; set; }
        public decimal SgstPercentage { get; set; }
        public decimal IgstPercentage { get; set; }
        public decimal CGST { get; set; }
        public decimal SGST { get; set; }
        public decimal IGST { get; set; }
        public decimal TaxAmount { get; set; }
        public int? PackTypeId { get; set; }
        public int? UOMId { get; set; }
        public decimal Charity { get; set; }
        public decimal HandlingCharges { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
