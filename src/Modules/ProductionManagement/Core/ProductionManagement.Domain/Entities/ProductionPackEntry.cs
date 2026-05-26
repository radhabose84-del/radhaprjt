using ProductionManagement.Domain.Common;

namespace ProductionManagement.Domain.Entities
{
    public class ProductionPackEntry : BaseEntity
    {
        public string? PackNo { get; set; }
        public DateOnly PackDate { get; set; }
        public int ProductionYear { get; set; } = DateTime.Now.Year;
        public int UnitId { get; set; }
        public int WarehouseId { get; set; }

        // Item identification
        public int ItemId { get; set; }
        public int? VariantId { get; set; }

        // Location & quality
        public int? BinId { get; set; }
        public int? QualityStatusId { get; set; }

        // Child collection — lot-level detail rows
        public ICollection<ProductionPackEntryDetail>? Details { get; set; }
    }
}
