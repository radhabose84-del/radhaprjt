namespace GateEntryManagement.Domain.Entities
{
    public class GateInwardDtl
    {
        public int Id { get; set; }
        public int GateInwardHdrId { get; set; }
        public int ReferenceDocTypeId { get; set; }       // Plain int (PO, JWO, Customer Return)
        public string? ReferenceDocNo { get; set; }
        public string? PartyName { get; set; }

        // Navigation
        public GateInwardHdr? GateInwardHdr { get; set; }
    }
}
