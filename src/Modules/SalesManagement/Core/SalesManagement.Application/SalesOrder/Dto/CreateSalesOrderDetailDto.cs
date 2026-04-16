namespace SalesManagement.Application.SalesOrder.Dto
{
    public class CreateSalesOrderDetailDto
    {
        public int ItemId { get; set; }
        public int? VariantId { get; set; }
        public int HSNId { get; set; }
        public int? PackTypeId { get; set; }
        public int QtyInBags { get; set; }
        public decimal BagWeight { get; set; }
        public int SaleUOMId { get; set; }
        public decimal TotalWeight { get; set; }
        public decimal ExMillRate { get; set; }
        public decimal DiscountPerUnit { get; set; }
        public decimal Freight { get; set; }
        public decimal? Handling { get; set; }
        public decimal? Charity { get; set; }
        public decimal TaxableAmount { get; set; }
        public decimal TaxPercentage { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TCSPercentage { get; set; }
        public decimal TCSAmount { get; set; }
        public decimal NetAmount { get; set; }
        public decimal NetRatePerKg { get; set; }
        public DateOnly ExpectedDeliveryDate { get; set; }
        public decimal AgentCommissionPercentage { get; set; }
    }
}
