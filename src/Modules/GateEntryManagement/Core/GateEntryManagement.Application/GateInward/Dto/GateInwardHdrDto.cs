namespace GateEntryManagement.Application.GateInward.Dto
{
    public class GateInwardHdrDto
    {
        public int Id { get; set; }
        public string? GateEntryNo { get; set; }
        public int VehicleMovementRecordId { get; set; }
        public string? VehicleMovementId { get; set; }
        public string? VehicleNumber { get; set; }
        public string? DriverName { get; set; }
        public decimal? GrossWeight { get; set; }
        public decimal? TareWeight { get; set; }
        public decimal? NetWeight { get; set; }
        public bool QAInspectionRequired { get; set; }
        public int? QAStatusId { get; set; }
        public string? QAStatusName { get; set; }
        public int UnitId { get; set; }
        public string? UnitName { get; set; }
        public string? Remarks { get; set; }

        // Single Gate Entry Document — AttachmentFilePath returns the composed
        // preview URL ({ImagePath}{GateEntryImage}/{AttachmentFileName})
        public string? AttachmentFileName { get; set; }
        public string? AttachmentFilePath { get; set; }

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

        public List<GateInwardDtlDto>? GateInwardDetails { get; set; }
    }
}
