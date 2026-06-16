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
    }
}
