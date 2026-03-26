namespace GateEntryManagement.Application.GatePass.Dto
{
    public class GatePassHdrDto
    {
        public int Id { get; set; }
        public string? GatePassNo { get; set; }
        public DateOnly GatePassDate { get; set; }
        public int VehicleMovementRecordId { get; set; }
        public string? VehicleMovementId { get; set; }
        public string? VehicleNumber { get; set; }
        public string? DriverName { get; set; }
        public string? DriverMobile { get; set; }
        public string? TransporterName { get; set; }
        public int UnitId { get; set; }
        public string? UnitName { get; set; }
        public int TotalItems { get; set; }
        public decimal TotalDocumentQty { get; set; }
        public decimal TotalDispatchQty { get; set; }
        public int? ReturnableItems { get; set; }
        public decimal TotalValue { get; set; }
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

        // Detail lines
        public List<GatePassDtlDto>? GatePassDetails { get; set; }
    }
}
