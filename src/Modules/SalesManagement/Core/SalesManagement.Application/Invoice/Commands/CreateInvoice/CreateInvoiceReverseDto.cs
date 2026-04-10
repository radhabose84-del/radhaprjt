namespace SalesManagement.Application.Invoice.Commands.CreateInvoice
{
    public class CreateInvoiceReverseDto
    {
        public InvoiceWorkFlowDto? Header { get; set; }
        public ICollection<InvoiceWorkFlowDto>? Lines { get; set; }
    }
}
