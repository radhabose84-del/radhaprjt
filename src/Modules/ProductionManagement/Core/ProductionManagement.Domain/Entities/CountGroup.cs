using ProductionManagement.Domain.Common;

namespace ProductionManagement.Domain.Entities
{
    public class CountGroup : BaseEntity
    {
        public string? CountGroupCode { get; set; }
        public string? CountGroupName { get; set; }
        public string? Description { get; set; }
    }
}
