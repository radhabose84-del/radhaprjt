using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class DiscountMaster : BaseEntity
    {
        public string? DiscountCode { get; set; }
        public string? DiscountName { get; set; }
        public int TriggerEventId { get; set; }
        public int DiscountBasisId { get; set; }
        public int ExecutionTypeId { get; set; }
        public int? CurrencyId { get; set; }
        public int? CustomerGroupId { get; set; }
        public int Priority { get; set; }
        public bool RequiresApproval { get; set; }
        public int? MaxDiscountLimitTypeId { get; set; }
        public decimal? MaxDiscountValue { get; set; }
        public bool IsStackable { get; set; }
        public int? ExclusionGroupId { get; set; }
        public int ValueTypeId { get; set; }
        public int SlabTypeId { get; set; }

        // Same-module navigation properties (MiscMaster)
        public MiscMaster? TriggerEvent { get; set; }
        public MiscMaster? DiscountBasis { get; set; }
        public MiscMaster? ExecutionType { get; set; }
        public MiscMaster? CustomerGroup { get; set; }
        public MiscMaster? MaxDiscountLimitType { get; set; }
        public MiscMaster? ExclusionGroup { get; set; }
        public MiscMaster? ValueType { get; set; }
        public MiscMaster? SlabType { get; set; }

        // Child collections
        public ICollection<DiscountSlab>? DiscountSlabs { get; set; }
        public ICollection<DiscountSalesGroup>? DiscountSalesGroups { get; set; }
        public ICollection<DiscountPaymentTerm>? DiscountPaymentTerms { get; set; }
    }
}
