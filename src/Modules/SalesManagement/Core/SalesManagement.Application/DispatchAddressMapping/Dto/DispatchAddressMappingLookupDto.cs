namespace SalesManagement.Application.DispatchAddressMapping.Dto
{
    public sealed class DispatchAddressMappingLookupDto
    {
        public int Id { get; set; }
        public int DispatchAddressId { get; set; }
        public string? DispatchAddressName { get; set; }
        public string? PartyName { get; set; }
    }
}
