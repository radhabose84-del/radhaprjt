namespace SalesManagement.Application.SalesQuotation.Commands.CreateSalesQuotation
{
    public class SalesQuotationWorkFlowDto
    {
        public int Id { get; set; }
        public string? QuotationNo { get; set; }
        public int? StatusId { get; set; }
        public string? StatusName { get; set; }
        public int? UnitId { get; set; }
    }
}
