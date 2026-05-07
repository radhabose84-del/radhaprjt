namespace SalesManagement.Application.SalesOrder.Dto
{
    /// <summary>
    /// Root response for the Approved Sales Order Discount report.
    /// Combines paged flat rows with global summaries grouped by agent and by customer.
    /// Pagination metadata lives on the wrapping <c>ApiResponseDTO</c>, not on this DTO.
    /// </summary>
    public class SalesOrderDiscountReportDto
    {
        /// <summary>Aggregates per (agent, payment term) across the full filtered dataset.</summary>
        public List<SalesOrderDiscountAgentSummaryDto> ByAgent { get; set; } = new();

        /// <summary>Aggregates per (customer, payment term) across the full filtered dataset.</summary>
        public List<SalesOrderDiscountCustomerSummaryDto> ByCustomer { get; set; } = new();

        /// <summary>Paged flat list of discount rows (slab + MD).</summary>
        public List<SalesOrderDiscountRowDto> Rows { get; set; } = new();
    }

    public class SalesOrderDiscountAgentSummaryDto
    {
        public int? AgentId { get; set; }
        public string? AgentName { get; set; }
        public int? SalesGroupId { get; set; }
        public string? SalesGroupName { get; set; }
        public int? PaymentTermId { get; set; }
        public string? PaymentTermName { get; set; }
        public int OrdersCount { get; set; }
        public decimal SlabDiscountValue { get; set; }
        public decimal MdDiscountValue { get; set; }
    }

    public class SalesOrderDiscountCustomerSummaryDto
    {
        public int PartyId { get; set; }
        public string? PartyName { get; set; }
        public int? SalesGroupId { get; set; }
        public string? SalesGroupName { get; set; }
        public int? PaymentTermId { get; set; }
        public string? PaymentTermName { get; set; }
        public int OrdersCount { get; set; }
        public decimal SlabDiscountValue { get; set; }
        public decimal MdDiscountValue { get; set; }
    }

    public class SalesOrderDiscountRowDto
    {
        public int SalesOrderId { get; set; }
        public string? SalesOrderNo { get; set; }
        public DateOnly OrderDate { get; set; }
        public int PartyId { get; set; }
        public string? PartyName { get; set; }
        public int? AgentId { get; set; }
        public string? AgentName { get; set; }

        /// <summary>"SLAB" (from Sales.SalesOrderDiscount) or "MD" (from Header.MdDiscountValue).</summary>
        public string DiscountSource { get; set; } = string.Empty;

        public int? DiscountMasterId { get; set; }
        public string? DiscountCode { get; set; }
        public string? DiscountName { get; set; }
        public string? SlabTypeName { get; set; }
        public decimal? DiscountRate { get; set; }
        public decimal? TotalDiscountValue { get; set; }

        /// <summary>
        /// Item lines on the underlying Sales Order. Populated even when the order has multiple
        /// lines — every line appears here so the report shows full per-line breakdown without
        /// inflating the discount totals.
        /// </summary>
        public List<SalesOrderDiscountRowLineDto> Details { get; set; } = new();
    }

    public class SalesOrderDiscountRowLineDto
    {
        public int? VariantId { get; set; }
        public string? ItemName { get; set; }
        public int? QtyInBags { get; set; }
        public decimal? BagWeight { get; set; }
        public int? SaleUOMId { get; set; }
        public string? UomName { get; set; }
        public decimal? TotalWeight { get; set; }
        public int? PackTypeId { get; set; }
        public string? PackTypeName { get; set; }
    }
}
