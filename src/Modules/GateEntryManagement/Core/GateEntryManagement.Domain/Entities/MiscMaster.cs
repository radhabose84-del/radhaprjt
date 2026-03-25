using GateEntryManagement.Domain.Common;

namespace GateEntryManagement.Domain.Entities
{
    public class MiscMaster : BaseEntity
    {
        public int MiscTypeId { get; set; }
        public string? Code { get; set; }
        public string? Description { get; set; }
        public int SortOrder { get; set; }

        public MiscTypeMaster? MiscTypeMaster { get; set; }

        // Reverse navigation (VehicleMovementRecord)
        public ICollection<VehicleMovementRecord>? VehicleMovementRecordsAsPurposeOfVisit { get; set; }
        public ICollection<VehicleMovementRecord>? VehicleMovementRecordsAsReferenceDocType { get; set; }
        public ICollection<VehicleMovementRecord>? VehicleMovementRecordsAsStatus { get; set; }
    }
}
