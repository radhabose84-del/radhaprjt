namespace FAM.Domain.Entities.AssetMaster
{
    public class AssetAudit
    {
        public int Id { get; set; }
        public string? AssetCode { get; set; }
        public string? AssetName { get; set; }
        public string? UnitName { get; set; }
        public string? Department { get; set; }
        public string? AuditorName { get; set; }
        public DateTimeOffset AuditDate { get; set; }
        public string? AuditFinancialYear { get; set; }
        public int? AuditTypeId { get; set; }
        public MiscMaster AuditTypeMiscType { get; set; } = null!;
        public int? UploadedFileId { get; set; }
        // Optional
        public string? SourceFileName { get; set; }
        public string? ScanType { get; set; }
        public string? Status { get; set; }
        public int UnitId { get; set; }
        public int? CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP { get; set; }

    }
}