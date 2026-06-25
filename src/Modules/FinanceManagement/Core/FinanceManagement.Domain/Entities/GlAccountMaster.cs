using FinanceManagement.Domain.Common;

namespace FinanceManagement.Domain.Entities
{
    public class GlAccountMaster : BaseEntity, IAuditTrailed
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
        public bool IsProfitCentreMandatory { get; set; }
        public bool IsTaxRelevant { get; set; }
        public bool IsInterCompany { get; set; }
        public bool IsReconciliationRequired { get; set; }

        // Traceability: the bulk-import run (GL02-FR-006) that created this account, if any.
        // Lets the import batch be activated in one call (AC3). NULL for manually-created accounts.
        public int? ImportLogId { get; set; }

        // US-GL02-08B (AC3) — stamped when a post-freeze change to this account is committed during an
        // unfreeze window. Non-NULL marks the account 'Post-Freeze' in COA listings/exports. NULL otherwise.
        public DateTimeOffset? LastPostFreezeChangeOn { get; set; }

        // ── US-GL02-10 Multi-Company COA (Shared vs Entity-specific) ──────────────────
        // True on the group/global template rows the Group FC maintains. New subsidiaries inherit
        // copies of these (AC1) and edits to a global row propagate to those copies (AC3).
        public bool IsGlobal { get; set; }

        // AC2 — account belongs to its owning entity only: never inherited/propagated, and a journal
        // line raised from any other company is rejected at validation. False = shareable.
        public bool IsCompanyRestricted { get; set; }

        // AC1/AC3 — on a per-company copy, points back to the IsGlobal template row it was inherited
        // from. NULL = entity-specific account created directly in the company (visible only there).
        public int? GlobalAccountId { get; set; }

        // AC3 — set true once an inherited copy has been edited locally, so global propagation skips
        // it ("an entity override exists"). False = the copy still tracks its global template.
        public bool IsLocalOverride { get; set; }

        // Same-module FK navigation
        public AccountTypeMaster? AccountTypeMaster { get; set; }
        public AccountGroup? AccountGroup { get; set; }
        public MiscMaster? NormalBalanceMaster { get; set; }
        public MiscMaster? SubLedgerTypeMaster { get; set; }

        // Currency type dropdown -> CurrencyForexConfig master (US-GL02-12)
        public CurrencyForexConfig? CurrencyTypeConfig { get; set; }

        // US-GL02-10 self-reference: global template (parent) -> its per-company copies (children)
        public GlAccountMaster? GlobalAccount { get; set; }
        public ICollection<GlAccountMaster>? CompanyCopies { get; set; }
    }
}
