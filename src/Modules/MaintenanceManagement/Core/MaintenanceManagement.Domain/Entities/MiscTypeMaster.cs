using MaintenanceManagement.Domain.Common;

namespace MaintenanceManagement.Domain.Entities
{
    public class MiscTypeMaster :BaseEntity
    {
        public string? MiscTypeCode { get; set; }
        public string? Description { get; set; }
        
        public ICollection<MiscMaster>? MiscMaster { get; set; }
    }
}