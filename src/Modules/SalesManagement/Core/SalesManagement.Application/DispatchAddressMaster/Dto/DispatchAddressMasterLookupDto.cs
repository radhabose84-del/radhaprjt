namespace SalesManagement.Application.DispatchAddressMaster.Dto
{
    public sealed class DispatchAddressMasterLookupDto
    {
        public int Id { get; set; }
        public string DispatchAddressName { get; set; } = default!;
        public string? AddressLine1 { get; set; }
        public int CityId { get; set; }
        public string? CityName { get; set; }
    }
}
