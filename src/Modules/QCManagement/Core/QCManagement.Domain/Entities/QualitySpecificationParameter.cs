using QCManagement.Domain.Common;

namespace QCManagement.Domain.Entities
{
    public class QualitySpecificationParameter : BaseEntity
    {
        public int QualitySpecificationId { get; set; }
        public int QualityParameterId { get; set; }
        public int ValidationTypeId { get; set; }
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        public string? ExpectedValue { get; set; }
        public string? AllowedValues { get; set; }
        public int? SeverityId { get; set; }
        public int? FailureActionId { get; set; }
        public bool IsSamplingRequired { get; set; }
        public string? Remarks { get; set; }

        // Navigation properties (same-module FKs only)
        public QualitySpecification? QualitySpecification { get; set; }
        public QualityParameter? QualityParameter { get; set; }
        public MiscMaster? ValidationType { get; set; }
        public MiscMaster? Severity { get; set; }
        public MiscMaster? FailureAction { get; set; }
    }
}
