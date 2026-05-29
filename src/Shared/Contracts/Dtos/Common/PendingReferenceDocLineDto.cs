namespace Contracts.Dtos.Common
{
    /// <summary>
    /// Per-PO container returned by <c>IPendingReferenceDocResolver.GetPendingItemsAsync</c>.
    /// One element per selected PO, each carrying its line items with quantities + tolerances.
    /// </summary>
    public sealed class PendingReferenceDocLineDto
    {
        public int DocId { get; set; }                   // PoId
        public string? DocNumber { get; set; }           // PONumber
        public DateTimeOffset DocDate { get; set; }
        public int PartyId { get; set; }
        public int UnitId { get; set; }
        public int POCategoryId { get; set; }
        public int POMethodId { get; set; }
        public byte IsPartialReceiptAllowed { get; set; }

        // Stamped by the handler from ITransactionTypeLookup.
        public int TransactionTypeId { get; set; }
        public string? DocumentTypeCode { get; set; }

        public List<PendingReferenceDocLineItemDto> Items { get; set; } = new();
    }

    public sealed class PendingReferenceDocLineItemDto
    {
        public int PoSlNo { get; set; }
        public int ItemId { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? UOMName { get; set; }
        public decimal? UpperTolerance { get; set; }
        public decimal? LowerTolerance { get; set; }
        public decimal? OrderQuantity { get; set; }
        public decimal? TotalGrnQty { get; set; }
        public decimal? PendingQty { get; set; }
    }
}
