namespace Contracts.Dtos.Common
{
    /// <summary>
    /// One row returned by an <see cref="Contracts.Interfaces.Gate.IPendingReferenceDocResolver"/>.
    /// Shape is uniform across all reference document types (LPO, IPO, CPO, EPO, BPO, ...).
    /// </summary>
    public sealed class PendingReferenceDocDto
    {
        public int DocId { get; set; }
        public string? DocNumber { get; set; }
        public DateTimeOffset DocDate { get; set; }
        public int PartyId { get; set; }
        public int UnitId { get; set; }
        public int TransactionTypeId { get; set; }
        public string? DocumentTypeCode { get; set; }

        // Filled by the orchestrating handler via IPartyLookup.
        public string? PartyName { get; set; }
    }
}
