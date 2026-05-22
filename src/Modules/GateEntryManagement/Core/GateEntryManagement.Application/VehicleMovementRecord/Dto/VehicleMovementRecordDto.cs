namespace GateEntryManagement.Application.VehicleMovementRecord.Dto
{
    public class VehicleMovementRecordDto
    {
        public int Id { get; set; }
        public string? VehicleMovementId { get; set; }
        public string? VehicleNumber { get; set; }
        public string? DriverName { get; set; }
        public string? DriverLicenseNo { get; set; }
        public string? DriverMobileNo { get; set; }
        public int? TransporterId { get; set; }
        public string? TransporterName { get; set; }
        public int PurposeOfVisitId { get; set; }
        public string? PurposeOfVisitName { get; set; }
        public int? ReferenceDocTypeId { get; set; }
        public string? ReferenceDocTypeName { get; set; }
        public string? ReferenceDocNo { get; set; }
        public DateTimeOffset GateInTime { get; set; }
        public string? GateInBy { get; set; }
        public DateTimeOffset? GateOutTime { get; set; }
        public string? GateOutBy { get; set; }
        public int StatusId { get; set; }
        public string? StatusName { get; set; }
        public int UnitId { get; set; }
        public string? UnitName { get; set; }
        public string? Remarks { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string? ModifiedByName { get; set; }
        public string? ModifiedIP { get; set; }
    }
}
