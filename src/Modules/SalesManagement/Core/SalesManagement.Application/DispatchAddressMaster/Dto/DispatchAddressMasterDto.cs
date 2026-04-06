namespace SalesManagement.Application.DispatchAddressMaster.Dto
{
    public class DispatchAddressMasterDto
    {
        public int Id { get; set; }
        public string? DispatchAddressName { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public int CityId { get; set; }
        public string? CityName { get; set; }
        public int StateId { get; set; }
        public string? StateName { get; set; }
        public int CountryId { get; set; }
        public string? CountryName { get; set; }
        public string? PinCode { get; set; }
        public string? ContactPerson { get; set; }
        public string? MobileNumber { get; set; }
        public string? Email { get; set; }
        public string? GSTIN { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public int? FreightId { get; set; }
        public string? FreightModeName { get; set; }
        public string? RateMethodName { get; set; }
        public decimal? FreightRate { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string? ModifiedByName { get; set; }
        public string? ModifiedIP { get; set; }
    }
}
