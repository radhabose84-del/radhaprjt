using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class DeliveryChallanHeader : BaseEntity
    {
        public string? DeliveryNumber { get; set; }
        public DateOnly DeliveryDate { get; set; }

        // STO Reference (same-module FK → Sales.StoHeader)
        public int StoHeaderId { get; set; }

        // From Plant & Storage Location (cross-module FKs)
        public int FromPlantId { get; set; }
        public int FromStorageLocationId { get; set; }

        // To Plant & Storage Location (cross-module FKs)
        public int ToPlantId { get; set; }
        public int ToStorageLocationId { get; set; }

        // Transporter (cross-module FK → PartyManagement)
        public int TransporterId { get; set; }
        public string? VehicleNumber { get; set; }
        public decimal? TransportDistance { get; set; }

        // Values
        public decimal DeliveryValue { get; set; }
        public decimal ConsignmentValue { get; set; }

        // Status (same-module FK → Sales.MiscMaster, MiscType = "DCLineStatus")
        public int StatusId { get; set; }

        // DC Type (same-module FK → Sales.MiscMaster, MiscType = "DCType": Returnable / Non-Returnable)
        public int DcTypeId { get; set; }

        // Movement Type (same-module FK → Sales.MovementTypeConfig)
        public int MovementTypeId { get; set; }

        public string? Remarks { get; set; }
        public bool GEFlag { get; set; }

        // Navigation properties (same-module FKs only)
        public StoHeader? StoHeader { get; set; }
        public MiscMaster? Status { get; set; }
        public MiscMaster? DcType { get; set; }
        public MovementTypeConfig? MovementType { get; set; }

        // Child collection
        public ICollection<DeliveryChallanDetail>? DeliveryChallanDetails { get; set; }

        // Reverse navigation (StoReceipt)
        public ICollection<StoReceiptHeader>? StoReceiptHeaders { get; set; }
    }
}
