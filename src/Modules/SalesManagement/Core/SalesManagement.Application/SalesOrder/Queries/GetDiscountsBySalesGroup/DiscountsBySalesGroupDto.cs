namespace SalesManagement.Application.SalesOrder.Queries.GetDiscountsBySalesGroup
{
    public sealed class DiscountsBySalesGroupDto
    {
        public int Id { get; set; }
        public string? DiscountCode { get; set; }
        public string? DiscountName { get; set; }
        public int Priority { get; set; }
        public int ExecutionTypeId { get; set; }
        public string? ExecutionTypeName { get; set; }
        public int TriggerEventId { get; set; }
        public string? TriggerEventName { get; set; }
        public int ValueTypeId { get; set; }
        public string? ValueTypeName { get; set; }

        public List<DiscountSalesGroupInfoDto> DiscountSalesGroups { get; set; } = [];
        public List<DiscountSlabInfoDto> DiscountSlabs { get; set; } = [];
        public List<DiscountPaymentTermInfoDto> DiscountPaymentTerms { get; set; } = [];
    }

    public sealed class DiscountSalesGroupInfoDto
    {
        public int Id { get; set; }
        public int SalesGroupId { get; set; }
        public string? SalesGroupName { get; set; }
    }

    public sealed class DiscountSlabInfoDto
    {
        public int Id { get; set; }
        public int SlabOrder { get; set; }
        public decimal FromValue { get; set; }
        public decimal? ToValue { get; set; }
        public decimal DiscountValue { get; set; }
    }

    public sealed class DiscountPaymentTermInfoDto
    {
        public int Id { get; set; }
        public int PaymentTermId { get; set; }
        public string? PaymentTermDescription { get; set; }
    }
}
