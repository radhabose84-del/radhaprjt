using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class StoHeader : BaseEntity
    {
        public string? StoNumber { get; set; }
        public DateOnly DocumentDate { get; set; }
        public DateOnly ExpectedDeliveryDate { get; set; }

        // STO Type (same-module FK → Sales.StoTypeMaster)
        public int StoTypeId { get; set; }

        // Movement Type (same-module FK → Sales.MovementTypeConfig)
        public int MovementTypeId { get; set; }

        // Supplying Plant & Storage Location (cross-module FKs)
        public int SupplyingPlantId { get; set; }
        public int SupplyingStorageLocationId { get; set; }

        // Receiving Plant & Storage Location (cross-module FKs)
        public int ReceivingPlantId { get; set; }
        public int ReceivingStorageLocationId { get; set; }

        public string? Remarks { get; set; }

        // Navigation properties (same-module FKs only)
        public StoTypeMaster? StoType { get; set; }
        public MovementTypeConfig? MovementType { get; set; }

        // Child collection
        public ICollection<StoDetail>? StoDetails { get; set; }
    }
}
