namespace Contracts.Dtos.Purchase
{
    /// <summary>
    /// Lean cross-module input used by GateEntryManagement to ask PurchaseManagement to validate
    /// and create a GRN row when a PO-backed Gate Inward is saved.
    /// </summary>
    /// <remarks>
    /// The header fields here are exactly what the user enters on the Gate Inward screen
    /// (invoice / DC / receiving warehouse / remarks). Each line carries the absolute minimum
    /// — <c>PoId</c>, <c>PoSlNoLocal</c>, <c>DcQuantity</c>. Everything else
    /// (<c>ItemId</c>, <c>OrderQuantity</c>, <c>UpperTolerance</c>, <c>LowerTolerance</c>,
    /// <c>POCategoryId</c>, <c>POMethodId</c>) is re-fetched server-side by the bridge from
    /// Purchase + Inventory tables — single source of truth.
    /// </remarks>
    public sealed class GateInwardGrnContextDto
    {
        public int PartyId { get; set; }
        public int UnitId { get; set; }
        /// <summary>Points at <c>Gate.GateInwardHdr.Id</c> (semantic shift from the legacy column).</summary>
        public int GateEntryId { get; set; }
        /// <summary>
        /// "LPO" / "IPO" / ... — drives which sub-table the bridge joins to enrich the lines.
        /// Resolved by the Gate handler via <c>ITransactionTypeLookup</c>.
        /// </summary>
        public string? DocumentTypeCode { get; set; }
        public string? InvoiceNo { get; set; }
        public DateTimeOffset InvoiceDate { get; set; }
        public string? DcNo { get; set; }
        public DateTimeOffset? DcDate { get; set; }
        public int ReceivingWarehouseId { get; set; }
        public string? Remarks { get; set; }
        public List<GateInwardGrnLineDto> Lines { get; set; } = new();
    }

    public sealed class GateInwardGrnLineDto
    {
        public int PoId { get; set; }
        public int? PoSlNoLocal { get; set; }
        public decimal DcQuantity { get; set; }
    }
}
