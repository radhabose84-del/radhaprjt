using UserManagement.Domain.Common;

namespace UserManagement.Domain.Entities
{
    public class MiscTypeMaster :BaseEntity
    {
        public int Id { get; set; }
        public string? MiscTypeCode { get; set; }
        public string? Description { get; set; }
        
        public ICollection<MiscMaster>? MiscMaster { get; set; }
    }
}