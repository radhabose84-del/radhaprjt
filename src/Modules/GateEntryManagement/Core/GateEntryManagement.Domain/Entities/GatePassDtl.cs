namespace GateEntryManagement.Domain.Entities
{
    public class GatePassDtl
    {
        public int Id { get; set; }
        public int GatePassHdrId { get; set; }
        public int DocTypeId { get; set; }
        public int DocId { get; set; }
        public string? DocNo { get; set; }
        public string? PartyName { get; set; }
        public string? PartyCode { get; set; }
        public DateOnly? DocDate { get; set; }
        public decimal TotalQty { get; set; }

        // Navigation
        public GatePassHdr? GatePassHdr { get; set; }
    }
}
