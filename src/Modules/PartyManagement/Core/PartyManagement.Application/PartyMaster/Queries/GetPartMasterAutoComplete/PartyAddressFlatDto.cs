namespace PartyManagement.Application.PartyMaster.Queries.GetPartMasterAutoComplete
{
    public class PartyAddressFlatDto
    {
        public int Id { get; set; }
        public int PartyId { get; set; }
        public string? AddressType { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? PostalCode { get; set; }
        public int? CityId { get; set; }
        public int? StateId { get; set; }
        public int? CountryId { get; set; }
        public int? LocationId { get; set; }
        public int? StationId { get; set; }
    }
}
