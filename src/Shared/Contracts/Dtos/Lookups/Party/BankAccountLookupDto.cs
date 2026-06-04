namespace Contracts.Dtos.Lookups.Party
{
    public sealed class BankAccountLookupDto
    {
        public int Id { get; set; }
        public string? AccountNumber { get; set; }
        public string? AccountHolderName { get; set; }
        public string? BankName { get; set; }
        public string? IFSCCode { get; set; }
        public string? SWIFTCode { get; set; }
        public string? IBan { get; set; }
        public int? AccountTypeId { get; set; }
        public string? AccountTypeName { get; set; }
        public int? BranchId { get; set; }
        public string? BranchName { get; set; }
        public bool IsDefaultAccount { get; set; }
        public bool IsPrimaryAccount { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public int? CityId { get; set; }
        public string? CityName { get; set; }
        public int? StateId { get; set; }
        public string? StateName { get; set; }
        public string? Pincode { get; set; }
    }
}
