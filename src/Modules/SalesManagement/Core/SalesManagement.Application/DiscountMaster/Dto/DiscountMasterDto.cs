namespace SalesManagement.Application.DiscountMaster.Dto
{
    public class DiscountMasterDto
    {
        public int Id { get; set; }
        public string? DiscountCode { get; set; }
        public string? DiscountName { get; set; }
        public int TriggerEventId { get; set; }
        public string? TriggerEventName { get; set; }
        public int DiscountBasisId { get; set; }
        public string? DiscountBasisName { get; set; }
        public int ExecutionTypeId { get; set; }
        public string? ExecutionTypeName { get; set; }
        public int? CurrencyId { get; set; }
        public string? CurrencyName { get; set; }
        public int? CustomerGroupId { get; set; }
        public string? CustomerGroupName { get; set; }
        public int Priority { get; set; }
        public bool RequiresApproval { get; set; }
        public int? MaxDiscountLimitTypeId { get; set; }
        public string? MaxDiscountLimitTypeName { get; set; }
        public decimal? MaxDiscountValue { get; set; }
        public bool IsStackable { get; set; }
        public int? ExclusionGroupId { get; set; }
        public string? ExclusionGroupName { get; set; }
        public int ValueTypeId { get; set; }
        public string? ValueTypeName { get; set; }
        public int SlabTypeId { get; set; }
        public string? SlabTypeName { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        // Child collections
        public List<DiscountSlabDto>? Slabs { get; set; }
        public List<DiscountSalesGroupDto>? SalesGroups { get; set; }
        public List<DiscountPaymentTermDto>? PaymentTerms { get; set; }

        // Audit fields
        public int CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string? ModifiedByName { get; set; }
        public string? ModifiedIP { get; set; }
    }

    public class DiscountSlabDto
    {
        public int Id { get; set; }
        public int SlabOrder { get; set; }
        public decimal FromValue { get; set; }
        public decimal? ToValue { get; set; }
        public decimal DiscountValue { get; set; }
    }

    public class DiscountSalesGroupDto
    {
        public int SalesGroupId { get; set; }
        public string? SalesGroupName { get; set; }
    }

    public class DiscountPaymentTermDto
    {
        public int PaymentTermId { get; set; }
        public string? PaymentTermDescription { get; set; }
    }

    public sealed class DiscountMasterLookupDto
    {
        public int Id { get; set; }
        public string? DiscountCode { get; set; }
        public string? DiscountName { get; set; }
    }
}
