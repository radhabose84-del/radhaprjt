namespace SalesManagement.Application.AgentPortal.Dto
{
    public class AgentSalesOrderListDto
    {
        public int Id { get; set; }
        public string? SalesOrderNo { get; set; }
        public DateOnly OrderDate { get; set; }
        public int PartyId { get; set; }
        public string? PartyName { get; set; }
        public string? SalesGroupName { get; set; }
        public string? StatusName { get; set; }
        public int TotalBags { get; set; }
        public decimal TotalWeightKgs { get; set; }
        public decimal FinalAmount { get; set; }
        public decimal TotalPendingQty { get; set; }
    }
}
