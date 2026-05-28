using QCManagement.Domain.Common;

namespace QCManagement.Domain.Entities
{
    public class QualityTemplateParameter : BaseEntity
    {
        public int QualityTemplateId { get; set; }
        public int QualityParameterId { get; set; }
        public int SequenceNo { get; set; }
        public bool IsMandatory { get; set; } = true;
        public bool IsCritical { get; set; }
        public int? InspectionMethodId { get; set; }
        public int? SampleSize { get; set; }
        public int? SampleUomId { get; set; }
        public bool IsGradeApplicable { get; set; }
        public string? Remarks { get; set; }

        // Navigation properties (same-module FKs only)
        public QualityTemplate? QualityTemplate { get; set; }
        public QualityParameter? QualityParameter { get; set; }
        public MiscMaster? InspectionMethod { get; set; }
        // Note: SampleUomId is a cross-module FK (Inventory.UOM) — no navigation property, no DB FK constraint
    }
}
