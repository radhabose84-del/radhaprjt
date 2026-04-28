namespace SalesManagement.Application.DispatchAdvice.Dto
{
    public class DispatchAdviceAddressDto
    {
        public int? Id { get; set; }
        public string? Source { get; set; }
        public int? DispatchAddressId { get; set; }
        public string? DispatchAddressName { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public int? CityId { get; set; }
        public string? CityName { get; set; }
        public int? StateId { get; set; }
        public string? StateName { get; set; }
        public int? CountryId { get; set; }
        public string? CountryName { get; set; }
        public string? PinCode { get; set; }
    }
}
