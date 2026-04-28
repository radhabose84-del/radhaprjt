namespace SalesManagement.Application.SalesQuotation.Commands.CreateSalesQuotationAmendment
{
    public class CreateSalesQuotationAmendmentReverseDto
    {
        public AmendmentWorkFlowDto? Header { get; set; }
        public ICollection<AmendmentWorkFlowDto>? Lines { get; set; }
    }
}
