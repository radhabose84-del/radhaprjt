namespace GateEntryManagement.Application.GateInward.Dto
{
    public class CreateGateInwardDetailDto
    {
        public int ReferenceDocTypeId { get; set; }
        public string? ReferenceDocNo { get; set; }
        public string? PartyName { get; set; }

        // Minimum PO context the FE sends per row.
        //   • PoId / PoSlNoLocal identify the PO line (FE got these from /pending-reference-doc-items)
        //   • DcQuantity is the user-entered receipt qty (the only irreducible input)
        // Other fields (ItemId, OrderQty, tolerances, POCategoryId, POMethodId) are re-fetched
        // server-side by the cross-module GRN bridge — no need to round-trip them.
        public int? PoId { get; set; }
        public int? PoSlNoLocal { get; set; }
        public decimal? DcQuantity { get; set; }
    }
}
