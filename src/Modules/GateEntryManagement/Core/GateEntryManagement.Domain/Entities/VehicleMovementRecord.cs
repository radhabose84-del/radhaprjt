using GateEntryManagement.Domain.Common;

namespace GateEntryManagement.Domain.Entities
{
    public class VehicleMovementRecord : BaseEntity
    {
        // System Generated
        public string? VehicleMovementId { get; set; }

        // Vehicle Details
        public string? VehicleNumber { get; set; }
        public string? DriverName { get; set; }
        public string? DriverLicenseNo { get; set; }
        public string? DriverMobileNo { get; set; }
        public int? TransporterId { get; set; }          // Cross-module FK (PartyManagement)

        // Basic Information
        public int PurposeOfVisitId { get; set; }         // Same-module FK → Gate.MiscMaster
        public int? ReferenceDocTypeId { get; set; }      // Same-module FK → Gate.MiscMaster
        public string? ReferenceDocNo { get; set; }

        // System Fields
        public DateTimeOffset GateInTime { get; set; }
        public string? GateInBy { get; set; }
        public DateTimeOffset? GateOutTime { get; set; }
        public string? GateOutBy { get; set; }
        public int StatusId { get; set; }                 // Same-module FK → Gate.MiscMaster

        // Additional
        public int UnitId { get; set; }                   // Cross-module FK (UserManagement)
        public string? Remarks { get; set; }

        // Navigation Properties (Same-Module FKs only)
        public MiscMaster? PurposeOfVisit { get; set; }
        public MiscMaster? ReferenceDocType { get; set; }
        public MiscMaster? StatusMisc { get; set; }

        // Reverse navigation (GatePass)
        public ICollection<GatePassHdr>? GatePassHeaders { get; set; }

        // Reverse navigation (GateInward)
        public ICollection<GateInwardHdr>? GateInwardHeaders { get; set; }
    }
}
