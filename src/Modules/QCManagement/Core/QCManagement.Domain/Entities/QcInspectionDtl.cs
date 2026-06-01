using QCManagement.Domain.Common;

namespace QCManagement.Domain.Entities
{
    public class QcInspectionDtl : BaseEntity, IActivityTracked
    {
        public int QcInspectionHdrId { get; set; }

        // Specification parameter snapshot (survives spec changes)
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

        // Inspector input + system-calculated result (NULL until parameter collection saved)
        public string? ActualValue { get; set; }
        public string? InspectionResult { get; set; }
        public string? Remarks { get; set; }

        // Parent
        public QcInspectionHdr? Hdr { get; set; }
    }
}
