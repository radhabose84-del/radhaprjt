using ProjectManagement.Domain.Common;

namespace ProjectManagement.Domain.Entities
{
    public class MiscMaster : BaseEntity
    {
         public int MiscTypeId { get; set; }
        public string? Code { get; set; }
        public string? Description { get; set; }
        public int SortOrder { get; set; }
        public MiscTypeMaster? MiscTypeMaster { get; set; }
    }
}