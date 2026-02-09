namespace Contracts.Dtos.Lookups.Party
{
    public class PartyLookupDto
    {
        public int Id { get; set; }
        public string PartyCode { get; set; } = string.Empty;
        public string PartyName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Mobile { get; set; }
    }
}
