using ProductionManagement.Domain.Common;

namespace ProductionManagement.Domain.Entities
{
    public class ProcessMaster : BaseEntity
    {
        public string? ProcessName { get; set; }
        public bool CombingRequired { get; set; }
        public string? Description { get; set; }
    }
}
