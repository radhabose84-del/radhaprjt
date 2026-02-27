using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class DispatchAddressMapping : BaseEntity
    {
        public int PartyId { get; set; }            // cross-module FK → PartyManagement (no navigation property)
        public int DispatchAddressId { get; set; }  // same-module FK → Sales.DispatchAddressMaster
        public int UsageTypeId { get; set; }        // same-module FK → Sales.MiscMaster
        public bool IsDefault { get; set; }

        // Same-module navigation properties
        public DispatchAddressMaster? DispatchAddress { get; set; }
        public MiscMaster? UsageType { get; set; }
    }
}
