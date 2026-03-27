using GateEntryManagement.Domain.Common;

namespace GateEntryManagement.Domain.Entities
{
    public class GateInwardHdr : BaseEntity
    {
        public string? GateEntryNo { get; set; }

        // Linked VMR
        public int VehicleMovementRecordId { get; set; }

        // Weighbridge
        public decimal? GrossWeight { get; set; }
        public decimal? TareWeight { get; set; }
        public decimal? NetWeight { get; set; }

        // QA
        public bool QAInspectionRequired { get; set; }
        public int? QAStatusId { get; set; }              // Same-module FK → Gate.MiscMaster

        // Additional
        public int UnitId { get; set; }                   // Cross-module FK (UserManagement)
        public string? Remarks { get; set; }

        // Navigation Properties
        public VehicleMovementRecord? VehicleMovementRecord { get; set; }
        public MiscMaster? QAStatusMisc { get; set; }

        // Child collection
        public ICollection<GateInwardDtl>? GateInwardDetails { get; set; }
    }
}
