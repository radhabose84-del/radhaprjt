namespace FinanceManagement.Application.GlAccountMaster.Dto
{
    public class GlAccountMasterLookupDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int AccountTypeId { get; set; }
        public string? AccountCode { get; set; }
        public string? AccountName { get; set; }
        public string? NormalBalanceCode { get; set; }

        // Level-3 sub-group ancestor of the account's group (e.g. an account on the L4
        // "Raw Material" leaf surfaces "Inventories"). Null if the group sits above L3.
        public int? L3AccountGroupId { get; set; }
        public string? L3GroupCode { get; set; }
        public string? L3GroupName { get; set; }
    }
}
