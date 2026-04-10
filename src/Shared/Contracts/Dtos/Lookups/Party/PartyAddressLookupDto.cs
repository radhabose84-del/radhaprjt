namespace Contracts.Dtos.Lookups.Party
{
    public sealed class PartyAddressLookupDto
    {
        public int Id { get; set; }
        public int? PartyId { get; set; }
        public string? AddressType { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public int? CityId { get; set; }
        public string? City { get; set; }
        public int? StateId { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public int? CountryId { get; set; }
        public string? Country { get; set; }
    }
}
