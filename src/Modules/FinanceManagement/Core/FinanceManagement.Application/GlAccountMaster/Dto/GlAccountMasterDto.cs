namespace FinanceManagement.Application.GlAccountMaster.Dto
{
    public class GlAccountMasterDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string? CompanyName { get; set; }

        public int AccountTypeId { get; set; }
        public string? AccountTypeName { get; set; }
        public string? StartCode { get; set; }
        public int AccountCodeLength { get; set; }

        public int AccountGroupId { get; set; }
        public string? AccountGroupCode { get; set; }
        public string? AccountGroupName { get; set; }

        public string? AccountCode { get; set; }
        public string? AccountName { get; set; }
        public string? Description { get; set; }

        public int NormalBalanceId { get; set; }
        public string? NormalBalanceCode { get; set; }
        public string? NormalBalanceName { get; set; }

        public int CurrencyTypeId { get; set; }
        public string? CurrencyTypeCode { get; set; }
        public string? CurrencyTypeName { get; set; }

        public int SubLedgerTypeId { get; set; }
        public string? SubLedgerTypeCode { get; set; }
        public string? SubLedgerTypeName { get; set; }

        public bool IsCostCentreMandatory { get; set; }
        public bool IsTaxRelevant { get; set; }
        public bool IsInterCompany { get; set; }
        public bool IsReconciliationRequired { get; set; }

        // Active tax-account linkage (LEFT JOIN Finance.TaxAccountLinkage; null when unlinked)
        public int? TaxAccountLinkageId { get; set; }
        public string? TaxCode { get; set; }
        public string? TaxName { get; set; }
        public string? ControlAccountType { get; set; }

        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public int CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string? ModifiedByName { get; set; }
        public string? ModifiedIP { get; set; }
    }
}
