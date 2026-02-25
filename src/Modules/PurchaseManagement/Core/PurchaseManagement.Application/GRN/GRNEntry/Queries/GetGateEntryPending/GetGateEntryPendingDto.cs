namespace PurchaseManagement.Application.GRN.GRNEntry.Queries.GetGateEntryPending
{
    public class GetGateEntryPendingDto
    {
        public int GateEntryId { get; set; }
        public int PoId { get; set; }
        public string? PONumber { get; set; }
        public string? GateEntryNo { get; set; }
        public DateTimeOffset? GateEntryDate { get; set; }
        
    }
}