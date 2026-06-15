namespace Contracts.Dtos.Lookups.Finance
{
    public sealed class AccountTypeMasterLookupDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string? AccountTypeName { get; set; }
        public string? StartCode { get; set; }
        public int AccountCodeLength { get; set; }
        public int SortOrder { get; set; }
    }
}
