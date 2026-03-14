namespace PartyManagement.Application.PartyMaster.Queries.GetPartMasterAutoComplete
{
    public class GetPartyMasterAutoCompleteDto
    {
        public int Id { get; set; }
        public string? PartyCode { get; set; }
        public string? PartyName { get; set; }
        public string? RegistrationType { get; set; }
        public string? GSTNumber { get; set; }
        public string? GSTFlag { get; set; }
        public string? PrimaryEmail { get; set; }
        public string? PrimaryMobile { get; set; }

        public List<PartyAddressAutoCompleteDto>? PartyAddresses { get; set; }
        public List<PartyContactAutoCompleteDto>? PartyContacts { get; set; }
    }

    public class PartyAddressAutoCompleteDto
    {
        public int Id { get; set; }
        public string? AddressType { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? PostalCode { get; set; }
        public int? CityId { get; set; }
        public string? CityName { get; set; }
        public int? StateId { get; set; }
        public string? StateName { get; set; }
        public int? CountryId { get; set; }
        public string? CountryName { get; set; }
    }

    public class PartyContactAutoCompleteDto
    {
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Designation { get; set; }
        public string? EmailID { get; set; }
        public string? MobileNo { get; set; }
        public string? Phone { get; set; }
        public int? GenderId { get; set; }
        public string? GenderName { get; set; }
        public int? PreferredChannelId { get; set; }
        public string? PreferredChannelName { get; set; }
        public int? ContactTypeId { get; set; }
        public string? ContactTypeName { get; set; }
        public string? ContactBy { get; set; }
    }
}