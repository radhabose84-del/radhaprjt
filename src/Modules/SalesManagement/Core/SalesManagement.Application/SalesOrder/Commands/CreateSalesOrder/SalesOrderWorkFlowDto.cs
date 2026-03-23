namespace SalesManagement.Application.SalesOrder.Commands.CreateSalesOrder
{
    public class SalesOrderWorkFlowDto
    {
        public int Id { get; set; }
        public string? SalesOrderNo { get; set; }
        public int? StatusId { get; set; }
        public string? StatusName { get; set; }
        public int UnitId { get; set; }
    }
}
