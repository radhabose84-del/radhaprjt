namespace SalesManagement.Application.AgentPortal.Dto
{
    public class AgentDispatchListDto
    {
        public int Id { get; set; }
        public string? DispatchNo { get; set; }
        public DateOnly DispatchDate { get; set; }
        public int PartyId { get; set; }
        public string? PartyName { get; set; }
        public string? SalesOrderNo { get; set; }
        public decimal TotDispatchedQty { get; set; }
        public string? StatusName { get; set; }
    }
}
