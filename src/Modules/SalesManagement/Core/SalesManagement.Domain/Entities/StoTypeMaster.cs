using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class StoTypeMaster : BaseEntity
    {
        public string? StoTypeCode { get; set; }
        public string? StoTypeName { get; set; }
        public string? Description { get; set; }

        // FK → Sales.MovementTypeConfig (PGI Movement Type)
        public int PgiMovementTypeId { get; set; }

        // FK → Sales.MovementTypeConfig (GR Movement Type)
        public int GrMovementTypeId { get; set; }

        // Navigation properties
        public MovementTypeConfig? PgiMovementType { get; set; }
        public MovementTypeConfig? GrMovementType { get; set; }

        // Reverse navigation (StoHeader)
        public ICollection<StoHeader>? StoHeaders { get; set; }
    }
}
