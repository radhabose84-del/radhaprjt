namespace FinanceManagement.Application.GlAccountImport.Dto
{
    /// <summary>
    /// All master/reference data needed to validate an import, loaded once per run (a handful of
    /// queries) so per-row validation is pure in-memory dictionary lookups. This is the key to the
    /// 500+ rows &lt; 60s requirement (AC4) — no per-row round-trips.
    /// Keys are upper-cased/trimmed codes; comparisons must use the matching normalisation.
    /// </summary>
    public sealed class GlAccountImportReferenceData
    {
        public int CompanyId { get; set; }

        // Existing groups for this company, keyed by upper(GroupCode).
        public Dictionary<string, ExistingGroupRef> GroupsByCode { get; set; } = new();

        // AccountType (statutory head) keyed by upper(AccountTypeName) and by id.
        public Dictionary<string, AccountTypeFormatRef> AccountTypesByName { get; set; } = new();
        public Dictionary<int, AccountTypeFormatRef> AccountTypesById { get; set; } = new();

        // Code-resolution maps (active, non-deleted, company-scoped where applicable).
        public Dictionary<string, int> NormalBalanceByCode { get; set; } = new();   // MiscMaster type NB
        public Dictionary<string, int> SubLedgerTypeByCode { get; set; } = new();   // MiscMaster type SLTYPE
        public Dictionary<string, int> CurrencyByCode { get; set; } = new();        // CurrencyForexConfig

        // Existing account code/name sets for duplicate checks (upper-cased).
        public HashSet<string> ExistingAccountCodes { get; set; } = new();
        public HashSet<string> ExistingAccountNames { get; set; } = new();
    }

    public sealed class ExistingGroupRef
    {
        public int Id { get; set; }
        public string GroupCode { get; set; } = null!;
        public int Level { get; set; }
        public int? ParentAccountGroupId { get; set; }
        public bool IsLeaf { get; set; }

        // Statutory head of this group's Level-1 ancestor — drives an attached account's
        // code length/prefix check (FR-001). Precomputed in the query repository.
        public int? RootAccountTypeId { get; set; }
    }

    public sealed class AccountTypeFormatRef
    {
        public int Id { get; set; }
        public string AccountTypeName { get; set; } = null!;
        public string? StartCode { get; set; }
        public int AccountCodeLength { get; set; }
    }
}
