namespace PurchaseManagement.Application.RawMaterialPO.Dto
{
    public class RawMaterialPODetailDto
    {
        public int Id { get; set; }
        public int POHeaderId { get; set; }

        public int ItemId { get; set; }
        public string? ItemName { get; set; }

        public int HsnId { get; set; }
        public string? HsnCode { get; set; }

        public decimal Quantity { get; set; }
        public decimal? Weight { get; set; }
        public decimal Rate { get; set; }

        public decimal ItemValue { get; set; }
        public decimal? CGSTPercentage { get; set; }
        public decimal? SGSTPercentage { get; set; }
        public decimal? IGSTPercentage { get; set; }
        public decimal? CGSTValue { get; set; }
        public decimal? SGSTValue { get; set; }
        public decimal? IGSTValue { get; set; }
        public decimal TotalGST { get; set; }
        public decimal NetValue { get; set; }
    }
}
