namespace SalesManagement.Application.AgentPortal.Dto
{
    public class AgentInvoiceListDto
    {
        public int Id { get; set; }
        public string? InvoiceNo { get; set; }
        public DateOnly InvoiceDate { get; set; }
        public int PartyId { get; set; }
        public string? PartyName { get; set; }
        public int TotalBags { get; set; }
        public decimal TotalWeight { get; set; }
        public decimal InvoiceAmount { get; set; }
        public string? StatusName { get; set; }
    }
}
