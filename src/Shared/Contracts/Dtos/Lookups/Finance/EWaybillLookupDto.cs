namespace Contracts.Dtos.Lookups.Finance
{
    public sealed class EWaybillLookupDto
    {
        public int Id { get; set; }
        public string? EWBNumber { get; set; }
        public string? EwbStatus { get; set; }
        public DateTimeOffset? GeneratedDate { get; set; }
    }
}
