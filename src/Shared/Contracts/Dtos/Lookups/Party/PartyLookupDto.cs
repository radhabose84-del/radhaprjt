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
        // Needed by cross-module callers that need a party's GSTIN (e.g. transporter
        // GSTIN for e-waybill generation). Nullable because not every party has one.
        public string? GstNumber { get; set; }
        public int? SalesFreightId { get; set; }
        public int? PurchaseFreightId { get; set; }
        public FreightMasterLookupDto? SalesFreight { get; set; }
        public FreightMasterLookupDto? PurchaseFreight { get; set; }
        public List<PartyAddressLookupDto>? Addresses { get; set; }
    }
}
