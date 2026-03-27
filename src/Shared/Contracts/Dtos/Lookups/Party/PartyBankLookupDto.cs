namespace Contracts.Dtos.Lookups.Party
{
    public sealed class PartyBankLookupDto
    {
        public string? BankName { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? BankBranch { get; set; }
        public string? IFSCCode { get; set; }
    }
}
