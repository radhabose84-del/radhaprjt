using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class ProductionPackHeader : BaseEntity
    {
        public string? PackNo { get; set; }
        public DateOnly PackDate { get; set; }
        public int UnitId { get; set; }
        public int WarehouseId { get; set; }
        public int TotalBags { get; set; }
        public decimal TotalNetWeight { get; set; }
        public string? Remarks { get; set; }

        // Child collection
        public ICollection<ProductionPackDetail>? ProductionPackDetails { get; set; }
    }
}
