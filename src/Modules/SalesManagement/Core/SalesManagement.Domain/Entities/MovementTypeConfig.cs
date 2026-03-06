using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class MovementTypeConfig : BaseEntity
    {
        public string? MovementCode { get; set; }
        public string? MovementDescription { get; set; }

        // FK → Sales.MiscMaster (MiscType = "MovementCategory")
        public int MovementCategoryId { get; set; }

        // FK → Sales.MiscMaster (MiscType = "StockType")
        public int FromStockTypeId { get; set; }

        // FK → Sales.MiscMaster (MiscType = "StockType") — must differ from FromStockTypeId
        public int ToStockTypeId { get; set; }

        public bool QuantityUpdateFlag { get; set; } = true;
        public bool ValueUpdateFlag { get; set; }
        public string? AccountModifier { get; set; }
        public bool BatchRequiredFlag { get; set; }
        public bool NegativeStockAllowed { get; set; }

        // Navigation properties
        public MiscMaster? MovementCategory { get; set; }
        public MiscMaster? FromStockType { get; set; }
        public MiscMaster? ToStockType { get; set; }

        // Reverse navigation — StoTypeMaster references this entity twice
        public ICollection<StoTypeMaster> StoTypeMastersAsPgi { get; set; } = new List<StoTypeMaster>();
        public ICollection<StoTypeMaster> StoTypeMastersAsGr { get; set; } = new List<StoTypeMaster>();

        // Reverse navigation (StoHeader)
        public ICollection<StoHeader>? StoHeaders { get; set; }
    }
}
