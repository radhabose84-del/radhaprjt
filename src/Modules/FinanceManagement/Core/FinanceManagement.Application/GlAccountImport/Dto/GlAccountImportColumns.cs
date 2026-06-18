namespace FinanceManagement.Application.GlAccountImport.Dto
{
    /// <summary>
    /// Single source of truth for the COA import/export layout (GL02-FR-006). One flat header row
    /// shared by both GROUP and ACCOUNT records, distinguished by the leading RecordType column.
    /// The template, the parser and the export writer all use these constants so an exported file
    /// re-imports cleanly (AC5).
    /// </summary>
    public static class GlAccountImportColumns
    {
        public const string RecordTypeGroup = "GROUP";
        public const string RecordTypeAccount = "ACCOUNT";

        // Ordered header — index in this array is the 0-based column position.
        public static readonly string[] Headers =
        {
            "RecordType",          // 0
            "GroupCode",           // 1  (GROUP)
            "GroupName",           // 2  (GROUP)
            "ParentGroupCode",     // 3  (GROUP; blank = Level-1 root)
            "AccountType",         // 4  (GROUP root: required; = AccountTypeMaster.AccountTypeName)
            "AccountCode",         // 5  (ACCOUNT)
            "AccountName",         // 6  (ACCOUNT)
            "Description",         // 7  (ACCOUNT)
            "AccountGroupCode",    // 8  (ACCOUNT; must be a leaf group)
            "NormalBalance",       // 9  (ACCOUNT; = MiscMaster.Code, type NB)
            "Currency",            // 10 (ACCOUNT; = CurrencyForexConfig.CurrencyTypeCode)
            "SubLedgerType",       // 11 (ACCOUNT; = MiscMaster.Code, type SLTYPE)
            "SortOrder",           // 12 (GROUP; optional, default 0)
            "IsCostCentreMandatory",   // 13 (ACCOUNT bool)
            "IsTaxRelevant",           // 14 (ACCOUNT bool)
            "IsInterCompany",          // 15 (ACCOUNT bool)
            "IsReconciliationRequired" // 16 (ACCOUNT bool)
        };

        public const int RecordType = 0;
        public const int GroupCode = 1;
        public const int GroupName = 2;
        public const int ParentGroupCode = 3;
        public const int AccountType = 4;
        public const int AccountCode = 5;
        public const int AccountName = 6;
        public const int Description = 7;
        public const int AccountGroupCode = 8;
        public const int NormalBalance = 9;
        public const int Currency = 10;
        public const int SubLedgerType = 11;
        public const int SortOrder = 12;
        public const int IsCostCentreMandatory = 13;
        public const int IsTaxRelevant = 14;
        public const int IsInterCompany = 15;
        public const int IsReconciliationRequired = 16;

        public const int ColumnCount = 17;

        // Two example rows shipped in the downloadable template to guide the Finance Controller.
        public static readonly string[] SampleGroupRow =
        {
            RecordTypeGroup, "1000", "Assets", "", "Asset", "", "", "", "", "", "", "", "1", "", "", "", ""
        };

        public static readonly string[] SampleAccountRow =
        {
            RecordTypeAccount, "", "", "", "", "100001", "Cash In Hand", "Petty cash", "1000",
            "DR", "INRONLY", "NONE", "", "0", "0", "0", "0"
        };
    }
}
