using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class DiscountMaster : BaseEntity
    {
        public string? DiscountCode { get; set; }
        public string? DiscountName { get; set; }
        public int DiscountTypeId { get; set; }
        public int ApplicableLevelId { get; set; }
        public int TriggerEventId { get; set; }
        public bool RequiresApproval { get; set; }
        public int? MaxDiscountLimitTypeId { get; set; }
        public int ValueTypeId { get; set; }
        public decimal? DiscountValue { get; set; }
        public int? SlabTypeId { get; set; }

        // Same-module navigation properties (MiscMaster)
        public MiscMaster? DiscountType { get; set; }
        public MiscMaster? ApplicableLevel { get; set; }
        public MiscMaster? TriggerEvent { get; set; }
        public MiscMaster? MaxDiscountLimitType { get; set; }
        public MiscMaster? ValueType { get; set; }
        public MiscMaster? SlabType { get; set; }

        // Child collections
        public ICollection<DiscountSlab>? DiscountSlabs { get; set; }
        public ICollection<DiscountSalesGroup>? DiscountSalesGroups { get; set; }
        public ICollection<DiscountPaymentTerm>? DiscountPaymentTerms { get; set; }
    }
}
