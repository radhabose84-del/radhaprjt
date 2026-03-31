namespace SalesManagement.Application.SalesOrder.Commands.CreateSalesOrderAmendment
{
    public class CreateSalesOrderAmendmentReverseDto
    {
        public AmendmentWorkFlowDto? Header { get; set; }
        public ICollection<AmendmentWorkFlowDto>? Lines { get; set; }
    }
}
