namespace SalesManagement.Application.SalesQuotation.Commands.CreateSalesQuotation
{
    public class CreateSalesQuotationReverseDto
    {
        public SalesQuotationWorkFlowDto? Header { get; set; }
        public ICollection<SalesQuotationWorkFlowDto>? Lines { get; set; }
    }
}
