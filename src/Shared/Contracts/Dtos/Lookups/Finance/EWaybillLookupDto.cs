namespace Contracts.Dtos.Lookups.Finance
{
    public sealed class EWaybillLookupDto
    {
        public string? EWBNumber { get; set; }
        public DateTimeOffset? GeneratedDate { get; set; }
    }
}
