using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class MiscMaster : BaseEntity
    {
        public int MiscTypeId { get; set; }
        public string Code { get; set; } = null!;
        public string Description { get; set; } = null!;
        public int SortOrder { get; set; }

        public MiscTypeMaster MiscTypeMaster { get; set; } = null!;

        // Reverse navigation
        public ICollection<DispatchAddressMapping>? DispatchAddressMappings { get; set; }
    }
}
