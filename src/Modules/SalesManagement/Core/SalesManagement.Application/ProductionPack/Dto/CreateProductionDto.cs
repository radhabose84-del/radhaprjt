namespace SalesManagement.Application.ProductionPack.Dto
{
    public class CreateProductionDto
    {
        public DateOnly PackDate { get; set; }
        public int ProductionYear { get; set; }
        public int WarehouseId { get; set; }
        public int TotalBags { get; set; }
        public decimal TotalNetWeight { get; set; }
        public decimal ProductionKgs { get; set; }
        public decimal LooseConeKgs { get; set; }
        public string? Remarks { get; set; }

        public List<CreateProductionPackDetailDto>? ProductionPackDetails { get; set; }
    }
}

