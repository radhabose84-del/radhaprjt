namespace Contracts.Dtos.Lookups.Users
{
    public sealed class UnitDetailLookupDto
    {
        public int UnitId { get; set; }
        public string? UnitName { get; set; }
        public string? CINNO { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public int CityId { get; set; }
        public int StateId { get; set; }
        public int PinCode { get; set; }
        public string? Phone { get; set; }
        public int? BankAccountId { get; set; }
    }
}
