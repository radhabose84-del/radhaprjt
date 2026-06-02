namespace QCManagement.Application.QcInspection.Dto
{
    /// <summary>GRN-level derived QC status (computed on demand).</summary>
    public class GrnQcStatusDto
    {
        public int GrnHeaderId { get; set; }
        public int TotalLines { get; set; }
        public int InspectedCount { get; set; }
        public int ApprovedCount { get; set; }
        public int ConditionallyApprovedCount { get; set; }
        public int HoldCount { get; set; }
        public int RejectedCount { get; set; }
        public string DerivedStatus { get; set; } = "PENDING_QC";
    }

    /// <summary>Snapshot of a Quality Specification (header + parameters) taken at inspection-create time.</summary>
    public class QcSpecSnapshotDto
    {
        public int QualitySpecificationId { get; set; }
        public string? QualitySpecificationCode { get; set; }
        public int QualityTemplateId { get; set; }
        public string? QualityTemplateCode { get; set; }
        public int QcTypeId { get; set; }
        public List<QcSpecParamSnapshotDto> Parameters { get; set; } = new();
    }

    public class QcSpecParamSnapshotDto
    {
        public int QualitySpecificationParameterId { get; set; }
        public int QualityParameterId { get; set; }
        public string? ParameterCode { get; set; }
        public string? ParameterName { get; set; }
        public int DataTypeId { get; set; }
        public int ValidationTypeId { get; set; }
        public string? ValidationTypeCode { get; set; }
        public int? UomId { get; set; }
        public string? UomCode { get; set; }
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        public string? ExpectedValue { get; set; }
        public string? AllowedValues { get; set; }
        public int SeverityId { get; set; }
        public string? SeverityCode { get; set; }
        public int FailureActionId { get; set; }
        public int SortOrder { get; set; }
    }

    /// <summary>Minimal per-detail snapshot needed to evaluate PASS/FAIL.</summary>
    public class QcInspectionDtlEvalDto
    {
        public int Id { get; set; }
        public string? ValidationTypeCode { get; set; }
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        public string? ExpectedValue { get; set; }
        public string? AllowedValues { get; set; }
    }

    /// <summary>Inspection summary keyed by GRN detail line — merged into the unified grid.</summary>
    public class QcInspectionSummaryDto
    {
        public int Id { get; set; }
        public int GrnDetailId { get; set; }
        public string? QcInspectionNo { get; set; }
        public int? QcStatusId { get; set; }
        public string? QcStatusCode { get; set; }
        public string? QcStatusName { get; set; }
        public decimal? AcceptedQuantity { get; set; }
        public decimal? RejectedQuantity { get; set; }
        public DateTimeOffset? InspectionDate { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string? ModifiedByName { get; set; }
    }
}
