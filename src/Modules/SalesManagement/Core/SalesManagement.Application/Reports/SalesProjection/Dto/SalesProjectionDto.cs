namespace SalesManagement.Application.Reports.SalesProjection.Dto
{
    public class SalesProjectionDto
    {
        public string? PeriodType { get; set; }
        public DateOnly? DateFrom { get; set; }
        public DateOnly? DateTo { get; set; }
        public List<SalesProjectionPeriodDto> Periods { get; set; } = new();
        public SalesProjectionSummaryDto? Summary { get; set; }
    }

    public class SalesProjectionPeriodDto
    {
        public string? PeriodLabel { get; set; }
        public DateOnly PeriodStart { get; set; }
        public DateOnly PeriodEnd { get; set; }
        public int LeadCount { get; set; }
        public int QuotationCount { get; set; }
        public decimal QuotationValue { get; set; }
        public int OrderCount { get; set; }
        public decimal OrderValue { get; set; }
        public int InvoicedCount { get; set; }
        public decimal InvoicedValue { get; set; }
        public decimal ProjectedRevenue { get; set; }
    }

    public class SalesProjectionSummaryDto
    {
        public int TotalLeads { get; set; }
        public int TotalQuotations { get; set; }
        public decimal TotalQuotationValue { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalOrderValue { get; set; }
        public int TotalInvoiced { get; set; }
        public decimal TotalInvoicedValue { get; set; }
        public decimal TotalProjectedRevenue { get; set; }
    }
}
