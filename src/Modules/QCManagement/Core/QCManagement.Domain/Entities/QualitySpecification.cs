using QCManagement.Domain.Common;

namespace QCManagement.Domain.Entities
{
    public class QualitySpecification : BaseEntity
    {
        public string? SpecificationCode { get; set; }
        public string? SpecificationName { get; set; }
        public int QualityTemplateId { get; set; }
        public int ApplicableLevelId { get; set; }
        public int? ItemCategoryId { get; set; }
        public int? ItemId { get; set; }
        public string? Description { get; set; }
        public DateTimeOffset EffectiveFrom { get; set; }
        public DateTimeOffset? EffectiveTo { get; set; }

        // Navigation properties (same-module FKs only)
        public QualityTemplate? QualityTemplate { get; set; }
        public MiscMaster? ApplicableLevel { get; set; }
        // Note: ItemCategoryId and ItemId are cross-module FKs (Inventory) — no navigation property, no DB FK constraint

        // Child collection
        public ICollection<QualitySpecificationParameter>? QualitySpecificationParameters { get; set; }
    }
}
