namespace Contracts.Dtos.Lookups.Party
{
    public sealed class PartyDetailLookupDto
    {
        public int Id { get; set; }
        public string? PartyCode { get; set; }
        public string? PartyName { get; set; }
        public string? GSTNumber { get; set; }
        public string? PAN { get; set; }
        public int? GSTStateCode { get; set; }
        public int CreditDays { get; set; }
        public bool IsGstReverseCharge { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public int CityId { get; set; }
        public int StateId { get; set; }
        public string? PostalCode { get; set; }
        public string? MobileNo { get; set; }
        public string? Phone { get; set; }
        public string? EmailID { get; set; }
    }
}
