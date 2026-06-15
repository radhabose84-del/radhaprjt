namespace FinanceManagement.Application.AccountGroup.Dto
{
    public sealed class AccountGroupLookupDto
    {
        public int Id { get; set; }
        public string? GroupCode { get; set; }
        public string? GroupName { get; set; }
        public int Level { get; set; }
    }
}
