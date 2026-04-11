using ProductionManagement.Domain.Common;

namespace ProductionManagement.Domain.Entities
{
    public class RawMaterialType : BaseEntity
    {
        public string? RawMaterialTypeCode { get; set; }
        public string? RawMaterialTypeName { get; set; }
        public string? Description { get; set; }
        public DateTimeOffset EffectiveFrom { get; set; }
        public DateTimeOffset? EffectiveTo { get; set; }
    }
}
