namespace SalesManagement.Application.SalesQuotation.Commands.CreateSalesQuotationAmendment
{
    public class AmendmentWorkFlowDto
    {
        public int Id { get; set; }
        public string? AmendmentNo { get; set; }
        public int SalesQuotationHeaderId { get; set; }
        public string? QuotationNo { get; set; }
        public int? StatusId { get; set; }
        public string? StatusName { get; set; }
        public int? UnitId { get; set; }
    }
}
