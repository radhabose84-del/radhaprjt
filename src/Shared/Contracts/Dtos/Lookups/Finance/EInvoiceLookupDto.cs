namespace Contracts.Dtos.Lookups.Finance
{
    public sealed class EInvoiceLookupDto
    {
        public int Id { get; set; }
        public string? IrnNumber { get; set; }
        public string? AckNo { get; set; }
        public DateTimeOffset? AckDate { get; set; }
    }
}
