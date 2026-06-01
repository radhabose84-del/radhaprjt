namespace GateEntryManagement.Domain.Entities
{
    public class GateInwardDtl
    {
        public int Id { get; set; }
        public int GateInwardHdrId { get; set; }
        public int ReferenceDocTypeId { get; set; }       // Plain int (PO, JWO, Customer Return)
        public string? ReferenceDocNo { get; set; }
        public string? PartyName { get; set; }

        // Minimum PO context — the only data not available elsewhere in the DB at GRN-build time.
        //   • PoId          — links back to the Purchase Order. Null/0 = non-PO line (manual receipt).
        //   • PoSlNoLocal   — line index within the PO (same item can repeat across lines).
        //   • DcQuantity    — user-entered receipt quantity. Null when the line is not PO-backed.
        // All three are nullable; the bridge skips lines lacking PoId or DcQuantity.
        public int? PoId { get; set; }
        public int? PoSlNoLocal { get; set; }
        public decimal? DcQuantity { get; set; }

        // Navigation
        public GateInwardHdr? GateInwardHdr { get; set; }
    }
}
