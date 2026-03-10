namespace SalesManagement.Application.ProductionPack.Dto
{
    public class UpdateProductionDto
    {
        public int Id { get; set; }
        public DateOnly PackDate { get; set; }
        public int WarehouseId { get; set; }
        public int TotalBags { get; set; }
        public decimal TotalNetWeight { get; set; }
        public decimal ProductionKgs { get; set; }
        public decimal LooseKgs { get; set; }
        public string? Remarks { get; set; }
        public int IsActive { get; set; }

        public List<UpdateProductionPackDetailDto>? ProductionPackDetails { get; set; }
    }
}
