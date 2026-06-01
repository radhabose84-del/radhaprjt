namespace PurchaseManagement.Infrastructure.Repositories.GateInward.Dto
{
    /// <summary>
    /// Flat Dapper projection row used by the LPO / IPO pending-line resolvers.
    /// Shared across resolvers since the SELECT shape is identical — only the FROM/JOIN tables differ.
    /// Resolver code groups these rows by <see cref="DocId"/> into <c>PendingReferenceDocLineDto</c>.
    /// </summary>
    internal sealed class PendingReferenceDocLineRow
    {
        public int DocId { get; set; }
        public string? DocNumber { get; set; }
        public DateTimeOffset DocDate { get; set; }
        public int PartyId { get; set; }
        public int UnitId { get; set; }
        public int POCategoryId { get; set; }
        public int POMethodId { get; set; }
        public byte IsPartialReceiptAllowed { get; set; }
        public int PoSlNo { get; set; }
        public int ItemId { get; set; }
        public decimal? OrderQuantity { get; set; }
        public decimal? TotalGrnQty { get; set; }
        public decimal? PendingQty { get; set; }
    }
}
