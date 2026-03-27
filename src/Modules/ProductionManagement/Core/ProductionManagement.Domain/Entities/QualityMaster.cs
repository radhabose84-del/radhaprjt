using ProductionManagement.Domain.Common;

namespace ProductionManagement.Domain.Entities
{
    public class QualityMaster : BaseEntity
    {
        public string? QualityName { get; set; }
        public string? Description { get; set; }
    }
}
