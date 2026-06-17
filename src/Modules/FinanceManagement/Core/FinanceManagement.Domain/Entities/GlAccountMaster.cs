using FinanceManagement.Domain.Common;

namespace FinanceManagement.Domain.Entities
{
    public class GlAccountMaster : BaseEntity
    {
        public int CompanyId { get; set; }
        public int AccountTypeId { get; set; }
        public int AccountGroupId { get; set; }

        public string? AccountCode { get; set; }
        public string? AccountName { get; set; }
        public string? Description { get; set; }

        public int NormalBalanceId { get; set; }
        public int CurrencyTypeId { get; set; }
        public int SubLedgerTypeId { get; set; }

        public bool IsCostCentreMandatory { get; set; }
        public bool IsTaxRelevant { get; set; }
        public bool IsInterCompany { get; set; }
        public bool IsReconciliationRequired { get; set; }

        // Same-module FK navigation
        public AccountTypeMaster? AccountTypeMaster { get; set; }
        public AccountGroup? AccountGroup { get; set; }
        public MiscMaster? NormalBalanceMaster { get; set; }
        public MiscMaster? SubLedgerTypeMaster { get; set; }

        // Currency type dropdown -> CurrencyForexConfig master (US-GL02-12)
        public CurrencyForexConfig? CurrencyTypeConfig { get; set; }
    }
}
