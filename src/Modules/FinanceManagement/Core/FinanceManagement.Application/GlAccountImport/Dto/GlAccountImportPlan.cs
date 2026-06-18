namespace FinanceManagement.Application.GlAccountImport.Dto
{
    /// <summary>A new account group the validator approved for creation.</summary>
    public sealed class PlannedAccountGroup
    {
        public int RowNumber { get; set; }
        public string GroupCode { get; set; } = null!;
        public string GroupName { get; set; } = null!;

        /// <summary>Code of the parent group (null = Level-1 root). Resolved to an id at commit time.</summary>
        public string? ParentGroupCode { get; set; }

        /// <summary>Existing parent id when the parent already lives in the DB; null when the parent
        /// is another new group from the same file (resolved during the topologically-ordered insert).</summary>
        public int? ExistingParentId { get; set; }

        public int? AccountTypeId { get; set; }   // root only
        public int SortOrder { get; set; }
        public int Level { get; set; }            // computed (1 = root)
    }

    /// <summary>A new GL account the validator approved for creation.</summary>
    public sealed class PlannedGlAccount
    {
        public int RowNumber { get; set; }
        public string AccountCode { get; set; } = null!;
        public string AccountName { get; set; } = null!;
        public string? Description { get; set; }

        /// <summary>Code of the owning leaf group. Resolved to an id at commit time
        /// (may be an existing group or one created earlier in this same run).</summary>
        public string AccountGroupCode { get; set; } = null!;
        public int? ExistingAccountGroupId { get; set; }

        public int AccountTypeId { get; set; }
        public int NormalBalanceId { get; set; }
        public int CurrencyTypeId { get; set; }
        public int SubLedgerTypeId { get; set; }

        public bool IsCostCentreMandatory { get; set; }
        public bool IsTaxRelevant { get; set; }
        public bool IsInterCompany { get; set; }
        public bool IsReconciliationRequired { get; set; }
    }

    /// <summary>Output of the two-pass validator.</summary>
    public sealed class GlAccountImportValidationResult
    {
        public int TotalRows { get; set; }
        public int GroupRows { get; set; }
        public int AccountRows { get; set; }

        /// <summary>New groups to create, already ordered parent-before-child (topological).</summary>
        public List<PlannedAccountGroup> Groups { get; set; } = new();

        /// <summary>Accounts to create.</summary>
        public List<PlannedGlAccount> Accounts { get; set; } = new();

        /// <summary>Every row/column failure found across both passes.</summary>
        public List<GlAccountImportErrorDto> Errors { get; set; } = new();

        public bool HasErrors => Errors.Count > 0;

        /// <summary>Distinct data rows (row &gt; 0) that produced at least one error.</summary>
        public int InvalidRowCount => Errors.Where(e => e.RowNumber > 0).Select(e => e.RowNumber).Distinct().Count();
    }
}
