using GateEntryManagement.Domain.Common;

namespace GateEntryManagement.Domain.Entities
{
    public class GatePassHdr : BaseEntity
    {
        public string? GatePassNo { get; set; }
        public DateOnly GatePassDate { get; set; }

        // VMR Reference
        public int VehicleMovementRecordId { get; set; }
        public string? VehicleNumber { get; set; }
        public string? DriverName { get; set; }
        public string? DriverMobile { get; set; }
        public string? TransporterName { get; set; }

        // Summary
        public int UnitId { get; set; }                   // Cross-module FK (UserManagement)
        public int TotalItems { get; set; }
        public decimal TotalDocumentQty { get; set; }
        public decimal TotalDispatchQty { get; set; }
        public int? ReturnableItems { get; set; }
        public decimal TotalValue { get; set; }

        // Weighbridge
        public decimal? GrossWeight { get; set; }
        public decimal? TareWeight { get; set; }
        public decimal? NetWeight { get; set; }

        public string? Remarks { get; set; }

        // Navigation Properties
        public VehicleMovementRecord? VehicleMovementRecord { get; set; }
        public ICollection<GatePassDtl>? GatePassDetails { get; set; }
    }
}
