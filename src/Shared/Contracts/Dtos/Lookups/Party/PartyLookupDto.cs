using Contracts.Dtos.Lookups.Logistics;

namespace Contracts.Dtos.Lookups.Party
{
    public class PartyLookupDto
    {
        public int Id { get; set; }
        public string PartyCode { get; set; } = string.Empty;
        public string PartyName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Mobile { get; set; }
        public int? SalesFreightId { get; set; }
        public int? PurchaseFreightId { get; set; }
        public FreightMasterLookupDto? SalesFreight { get; set; }
        public FreightMasterLookupDto? PurchaseFreight { get; set; }
        public List<PartyAddressLookupDto>? Addresses { get; set; }
    }
}
