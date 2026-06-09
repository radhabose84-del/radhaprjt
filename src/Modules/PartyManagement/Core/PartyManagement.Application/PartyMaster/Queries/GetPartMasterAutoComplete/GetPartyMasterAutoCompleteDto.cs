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
        public List<SalesTypeAutoCompleteDto>? SalesTypes { get; set; }
        public List<TransportDetailAutoCompleteDto>? TransportDetails { get; set; }
    }

    public class SalesTypeAutoCompleteDto
    {
        public int Id { get; set; }
        public int? SalesSegmentId { get; set; }
        public string? SegmentName { get; set; }
        public int? PaymentTypeId { get; set; }
        public string? PaymentTypeName { get; set; }
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

    public class TransportDetailAutoCompleteDto
    {
        public int Id { get; set; }
        public int PartyId { get; set; }
        public int? TransporterTypeId { get; set; }
        public string? TransporterTypeName { get; set; }
        public int? TransportModeId { get; set; }
        public string? TransportModeName { get; set; }
        public int? VehicleTypeId { get; set; }
        public string? VehicleTypeName { get; set; }
        public int? DefaultFreightTypeId { get; set; }
        public string? DefaultFreightTypeName { get; set; }
        public decimal? DefaultFreightRate { get; set; }
        public decimal? MinFreightAmount { get; set; }
        public string? LicenseNo { get; set; }
        public DateTimeOffset? LicenseExpiryDate { get; set; }
        public string? VehicleNo { get; set; }
        public string? InsuranceProvider { get; set; }
        public string? PolicyNo { get; set; }
        public DateTimeOffset? InsuranceExpiryDate { get; set; }
        // Cross-module — ModuleName populated post-query via IModuleLookup.
        public int? ModuleId { get; set; }
        public string? ModuleName { get; set; }
        // Same-module — DefaultProcurementRateBasisName populated via SQL JOIN to Party.MiscMaster.
        public int? DefaultProcurementRateBasisId { get; set; }
        public string? DefaultProcurementRateBasisName { get; set; }
        public byte Status { get; set; }
    }

    public class PartyContactAutoCompleteDto
    {
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Designation { get; set; }
        public string? EmailID { get; set; }
        public string? AlternateEmailId { get; set; }
        public string? MobileNo { get; set; }
        public string? AlternateMobileNumber { get; set; }
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