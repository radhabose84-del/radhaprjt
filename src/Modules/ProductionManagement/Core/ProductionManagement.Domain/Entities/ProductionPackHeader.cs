using ProductionManagement.Domain.Common;

namespace ProductionManagement.Domain.Entities
{
    public class ProductionPackHeader : BaseEntity
    {
        public string? PackNo { get; set; }
        public DateOnly PackDate { get; set; }
        public int ProductionYear { get; set; } = DateTime.Now.Year;
        public int UnitId { get; set; }
        public int WarehouseId { get; set; }
        public int TotalBags { get; set; }
        public decimal TotalNetWeight { get; set; }
        public decimal ProductionKgs { get; set; }
        public decimal LooseConeKgs { get; set; }
        public string? Remarks { get; set; }

        // Child collection
        public ICollection<ProductionPackDetail>? ProductionPackDetails { get; set; }
    }
}
