namespace Contracts.Dtos.Lookups.Gate
{
    /// <summary>
    /// Lookup row for a centralized Gate Inward record. Used cross-module to resolve
    /// <c>GateEntryNo</c> + <c>GateEntryDate</c> from a <c>GateInwardHdr.Id</c>.
    /// </summary>
    public sealed class GateInwardLookupDto
    {
        public int Id { get; set; }
        public string? GateEntryNo { get; set; }
        public DateTimeOffset? GateEntryDate { get; set; }
    }
}
