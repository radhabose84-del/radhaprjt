namespace SalesManagement.Application.Production.Dto
{
    public class CreateProductionDto
    {
        public DateOnly PackDate { get; set; }
        public int UnitId { get; set; }
        public int WarehouseId { get; set; }
        public int StatusId { get; set; }
        public int TotalBags { get; set; }
        public decimal TotalNetWeight { get; set; }
        public string? Remarks { get; set; }

        public List<CreateProductionPackDetailDto>? ProductionPackDetails { get; set; }
    }
}
