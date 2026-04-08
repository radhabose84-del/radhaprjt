namespace ProductionManagement.Application.ProductionPack.Dto
{
    public class ProductionStockRegisterDto
    {
        public int Id { get; set; }
        public int UnitId { get; set; }
        public string? UnitName { get; set; }
        public int ItemId { get; set; }
        public string? ItemName { get; set; }
        public int LotId { get; set; }
        public string? LotCode { get; set; }
        public DateOnly DocDate { get; set; }
        public decimal OpeningLooseKgs { get; set; }
        public decimal ProdKgs { get; set; }
        public decimal TotalProdKgs { get; set; }
        public int PackTypeId { get; set; }
        public string? PackTypeName { get; set; }
        public decimal NetWeightPerPack { get; set; }
        public int TotalBags { get; set; }
        public decimal NetWeight { get; set; }
        public int BagsRepacked { get; set; }
        public decimal RepackKgs { get; set; }
        public decimal ClosingLooseKgs { get; set; }
        public decimal ClosingPackKgs { get; set; }
        public int ClosingBags { get; set; }
        public bool StockClosing { get; set; }
    }
}
