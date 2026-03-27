using ProductionManagement.Domain.Common;

namespace ProductionManagement.Domain.Entities
{
    public class CertificationMaster : BaseEntity
    {
        public string? CertificationName { get; set; }
        public string? Description { get; set; }
    }
}
