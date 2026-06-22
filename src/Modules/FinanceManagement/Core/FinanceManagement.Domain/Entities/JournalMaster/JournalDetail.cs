using FinanceManagement.Domain.Common;

namespace FinanceManagement.Domain.Entities
{
    // US-GL01-01/05/09 — journal voucher lines. Each line is a fully-coded debit or credit with
    // currency, cost centre, profit centre, narration and reference doc. Base amounts feed LedgerBalance.
    public class JournalDetail : BaseEntity
    {
        public int JournalHeaderId { get; set; }        // same-module FK -> JournalHeader
        public int LineNo { get; set; }

        public int GlAccountId { get; set; }            // same-module FK -> GlAccountMaster
        public decimal DrAmount { get; set; }
        public decimal CrAmount { get; set; }

        public int CurrencyId { get; set; }             // same-module FK -> CurrencyForexConfig (transaction currency)
        public decimal? ExchangeRate { get; set; }      // to company base currency
        public decimal? BaseDrAmount { get; set; }      // = DrAmount * ExchangeRate; feeds LedgerBalance
        public decimal? BaseCrAmount { get; set; }      // = CrAmount * ExchangeRate; feeds LedgerBalance

        public int? CostCentreId { get; set; }          // same-module FK -> CostCentre (required when account IsCostCentreMandatory)
        public int? ProfitCentreId { get; set; }        // same-module FK -> ProfitCentre
        public string? LineNarration { get; set; }
        public string? ReferenceDocNo { get; set; }     // per-line external doc ref (searchable)

        // Same-module FK navigation
        public JournalHeader? JournalHeader { get; set; }
        public GlAccountMaster? GlAccount { get; set; }
        public CurrencyForexConfig? Currency { get; set; }
        public CostCentre? CostCentre { get; set; }
        public ProfitCentre? ProfitCentre { get; set; }
    }
}
