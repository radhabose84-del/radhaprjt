namespace SalesManagement.Application.SalesOrder.Commands.CreateSalesOrderAmendment
{
    public class AmendmentWorkFlowDto
    {
        public int Id { get; set; }
        public string? AmendmentNo { get; set; }
        public int SalesOrderHeaderId { get; set; }
        public string? SalesOrderNo { get; set; }
        public int? StatusId { get; set; }
        public string? StatusName { get; set; }
        public int? UnitId { get; set; }
    }
}
