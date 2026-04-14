namespace SalesManagement.Application.SalesOrder.Dto
{
    /// <summary>
    /// Intermediate row shapes for the GetDiscountsBySalesGroup query.
    /// Used by the Dapper query in SalesOrderQueryRepository; not exposed as response DTOs.
    /// </summary>
    public sealed class DiscountSalesGroupRow
    {
        public int Id { get; set; }
        public int DiscountMasterId { get; set; }
        public int SalesGroupId { get; set; }
        public string? SalesGroupName { get; set; }
    }

    public sealed class DiscountSlabRow
    {
        public int Id { get; set; }
        public int DiscountMasterId { get; set; }
        public int SlabOrder { get; set; }
        public decimal FromValue { get; set; }
        public decimal? ToValue { get; set; }
        public decimal DiscountValue { get; set; }
    }

    public sealed class DiscountPaymentTermRow
    {
        public int Id { get; set; }
        public int DiscountMasterId { get; set; }
        public int PaymentTermId { get; set; }
    }
}
