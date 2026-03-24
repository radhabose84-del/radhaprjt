namespace SalesManagement.Application.SalesOrder.Commands.CreateSalesOrder
{
    public class CreateSalesOrderReverseDto
    {
        public SalesOrderWorkFlowDto? Header { get; set; }
        public ICollection<SalesOrderWorkFlowDto>? Lines { get; set; }
    }
}
