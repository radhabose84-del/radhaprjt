namespace ProductionManagement.Application.ProductionPack.Dto
{
    public class ProductionStockClosingDto
    {
        public decimal ClosingLooseKgs { get; set; }
        public decimal ClosingPackKgs { get; set; }
        public int ClosingBags { get; set; }
        public DateOnly ProdDate { get; set; }
    }
}
