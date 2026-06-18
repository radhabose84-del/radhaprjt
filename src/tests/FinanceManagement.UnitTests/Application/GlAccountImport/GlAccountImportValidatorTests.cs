using System;
using System.Linq;
using FinanceManagement.Application.GlAccountImport.Dto;
using FinanceManagement.Application.GlAccountImport.Services;

namespace FinanceManagement.UnitTests.Application.GlAccountImport
{
    /// <summary>
    /// Unit tests for the two-pass COA import validator (GL02-FR-006). Pure in-memory — the
    /// validator takes pre-loaded reference data, so no mocks/DB are needed.
    /// </summary>
    public sealed class GlAccountImportValidatorTests
    {
        private static readonly GlAccountImportValidator Validator = new();
        private static readonly System.Collections.Generic.List<GlAccountImportErrorDto> NoParseErrors = new();

        // ── reference data + row builders ─────────────────────────────────────
        private static GlAccountImportReferenceData NewRef()
        {
            var asset = new AccountTypeFormatRef { Id = 10, AccountTypeName = "Asset", StartCode = "1", AccountCodeLength = 6 };
            var data = new GlAccountImportReferenceData { CompanyId = 1 };
            data.AccountTypesById[asset.Id] = asset;
            data.AccountTypesByName["ASSET"] = asset;
            data.NormalBalanceByCode["DR"] = 1;
            data.NormalBalanceByCode["CR"] = 2;
            data.SubLedgerTypeByCode["NONE"] = 3;
            data.CurrencyByCode["INRONLY"] = 4;
            return data;
        }

        private static GlAccountImportRowDto G(int row, string code, string name, string? parent = null, string? type = null, string sort = "0")
            => new()
            {
                RowNumber = row,
                RecordType = "GROUP",
                GroupCode = code,
                GroupName = name,
                ParentGroupCode = parent,
                AccountType = type,
                SortOrder = sort
            };

        private static GlAccountImportRowDto A(int row, string code, string name, string group,
            string nb = "DR", string cur = "INRONLY", string slt = "NONE", string? desc = null)
            => new()
            {
                RowNumber = row,
                RecordType = "ACCOUNT",
                AccountCode = code,
                AccountName = name,
                AccountGroupCode = group,
                NormalBalance = nb,
                Currency = cur,
                SubLedgerType = slt,
                Description = desc
            };

        private static GlAccountImportValidationResult Run(GlAccountImportReferenceData refData, params GlAccountImportRowDto[] rows)
            => Validator.Validate(rows, NoParseErrors, refData);

        // ── happy path ────────────────────────────────────────────────────────
        [Fact]
        public void Valid_tree_with_account_plans_everything_without_errors()
        {
            var result = Run(NewRef(),
                G(2, "1000", "Assets", parent: null, type: "Asset"),
                G(3, "1100", "Current Assets", parent: "1000"),
                A(4, "110001", "Cash", group: "1100"));

            result.HasErrors.Should().BeFalse();
            result.Groups.Should().HaveCount(2);
            result.Accounts.Should().ContainSingle(a => a.AccountCode == "110001");
            // Parent before child (topological order).
            result.Groups.First().GroupCode.Should().Be("1000");
            // Account inherits its group's Level-1 statutory head.
            result.Accounts.Single().AccountTypeId.Should().Be(10);
        }

        // ── hierarchy / loop detection ────────────────────────────────────────
        [Fact]
        public void Self_referencing_group_is_rejected_as_a_loop()
        {
            var result = Run(NewRef(), G(2, "1000", "Assets", parent: "1000"));

            result.Groups.Should().BeEmpty();
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("own parent"));
        }

        [Fact]
        public void Two_group_cycle_is_detected()
        {
            var result = Run(NewRef(),
                G(2, "AAA", "A", parent: "BBB"),
                G(3, "BBB", "B", parent: "AAA"));

            result.Groups.Should().BeEmpty();
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("loop"));
        }

        [Fact]
        public void Group_beyond_max_depth_of_six_is_rejected()
        {
            var rows = new System.Collections.Generic.List<GlAccountImportRowDto>
            {
                G(2, "L1", "Level1", parent: null, type: "Asset")
            };
            for (int i = 2; i <= 7; i++)
                rows.Add(G(i + 1, $"L{i}", $"Level{i}", parent: $"L{i - 1}"));

            var result = Validator.Validate(rows.ToArray(), NoParseErrors, NewRef());

            result.Groups.Should().HaveCount(6);                 // L1..L6 ok
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("maximum hierarchy depth"));
        }

        [Fact]
        public void Group_with_unknown_parent_is_rejected()
        {
            var result = Run(NewRef(), G(2, "1100", "Current Assets", parent: "NOPE"));

            result.Groups.Should().BeEmpty();
            result.Errors.Should().Contain(e => e.ColumnName == "ParentGroupCode" && e.ErrorMessage.Contains("does not exist"));
        }

        [Fact]
        public void Root_group_requires_account_type()
        {
            var result = Run(NewRef(), G(2, "1000", "Assets", parent: null, type: null));

            result.Errors.Should().Contain(e => e.ColumnName == "AccountType" && e.ErrorMessage.Contains("required"));
        }

        [Fact]
        public void Sub_group_must_not_carry_account_type()
        {
            var result = Run(NewRef(),
                G(2, "1000", "Assets", parent: null, type: "Asset"),
                G(3, "1100", "Current", parent: "1000", type: "Asset"));

            result.Errors.Should().Contain(e => e.ColumnName == "AccountType" && e.ErrorMessage.Contains("only allowed"));
        }

        [Fact]
        public void Duplicate_group_code_in_file_is_rejected()
        {
            var result = Run(NewRef(),
                G(2, "1000", "Assets", parent: null, type: "Asset"),
                G(3, "1000", "Assets Dup", parent: null, type: "Asset"));

            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Duplicate GroupCode"));
        }

        [Fact]
        public void Existing_group_in_db_is_skipped_not_recreated()
        {
            var refData = NewRef();
            refData.GroupsByCode["1000"] = new ExistingGroupRef
            { Id = 900, GroupCode = "1000", Level = 1, ParentAccountGroupId = null, IsLeaf = true, RootAccountTypeId = 10 };

            var result = Run(refData, G(2, "1000", "Assets", parent: null, type: "Asset"));

            result.HasErrors.Should().BeFalse();
            result.Groups.Should().BeEmpty();   // already exists → no-op
        }

        // ── account rules ─────────────────────────────────────────────────────
        [Fact]
        public void Account_on_non_leaf_group_is_rejected()
        {
            // 1000 has a child (1100) → 1000 is not a leaf; attaching an account to it must fail.
            var result = Run(NewRef(),
                G(2, "1000", "Assets", parent: null, type: "Asset"),
                G(3, "1100", "Current", parent: "1000"),
                A(4, "100001", "Cash", group: "1000"));

            result.Errors.Should().Contain(e => e.ColumnName == "AccountGroupCode" && e.ErrorMessage.Contains("leaf"));
        }

        [Fact]
        public void Duplicate_account_code_in_file_is_rejected()
        {
            var result = Run(NewRef(),
                G(2, "1000", "Assets", parent: null, type: "Asset"),
                A(3, "100001", "Cash", group: "1000"),
                A(4, "100001", "Cash 2", group: "1000"));

            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Duplicate AccountCode"));
        }

        [Fact]
        public void Existing_account_code_in_db_is_skipped_for_clean_reimport()
        {
            var refData = NewRef();
            refData.GroupsByCode["1000"] = new ExistingGroupRef
            { Id = 900, GroupCode = "1000", Level = 1, ParentAccountGroupId = null, IsLeaf = true, RootAccountTypeId = 10 };
            refData.ExistingAccountCodes.Add("100001");
            refData.ExistingAccountNames.Add("CASH");

            var result = Run(refData,
                G(2, "1000", "Assets", parent: null, type: "Asset"),
                A(3, "100001", "Cash", group: "1000"));

            result.HasErrors.Should().BeFalse();      // AC5 — full export re-imports cleanly
            result.Accounts.Should().BeEmpty();
        }

        [Fact]
        public void Account_with_unknown_currency_is_rejected()
        {
            var result = Run(NewRef(),
                G(2, "1000", "Assets", parent: null, type: "Asset"),
                A(3, "100001", "Cash", group: "1000", cur: "ZZZ"));

            result.Errors.Should().Contain(e => e.ColumnName == "Currency" && e.ErrorMessage.Contains("does not exist"));
        }

        [Fact]
        public void Account_code_must_be_digits_only()
        {
            var result = Run(NewRef(),
                G(2, "1000", "Assets", parent: null, type: "Asset"),
                A(3, "10A001", "Cash", group: "1000"));

            result.Errors.Should().Contain(e => e.ColumnName == "AccountCode" && e.ErrorMessage.Contains("digits only"));
        }

        [Fact]
        public void Account_code_length_and_prefix_follow_the_account_type()
        {
            // Asset type: length 6, prefix "1".
            var wrongLength = Run(NewRef(),
                G(2, "1000", "Assets", parent: null, type: "Asset"),
                A(3, "1001", "Short", group: "1000"));
            wrongLength.Errors.Should().Contain(e => e.ErrorMessage.Contains("exactly 6 digits"));

            var wrongPrefix = Run(NewRef(),
                G(2, "1000", "Assets", parent: null, type: "Asset"),
                A(3, "900001", "BadPrefix", group: "1000"));
            wrongPrefix.Errors.Should().Contain(e => e.ErrorMessage.Contains("must start with '1'"));
        }

        [Fact]
        public void Unknown_record_type_is_reported()
        {
            var result = Run(NewRef(), new GlAccountImportRowDto { RowNumber = 2, RecordType = "WIDGET" });

            result.Errors.Should().Contain(e => e.ColumnName == "RecordType");
        }

        [Fact]
        public void All_errors_are_collected_no_early_stop()
        {
            // Three independently-bad account rows must each surface (AC1: validate all before commit).
            var result = Run(NewRef(),
                G(2, "1000", "Assets", parent: null, type: "Asset"),
                A(3, "10A001", "A", group: "1000"),       // non-digit
                A(4, "100002", "B", group: "MISSING"),    // unknown group
                A(5, "100003", "C", group: "1000", cur: "ZZZ")); // bad currency

            result.InvalidRowCount.Should().Be(3);
            result.Accounts.Should().BeEmpty();
        }

        [Fact]
        public void Account_under_a_group_whose_parent_was_invalid_is_not_planned()
        {
            // 2000 references a missing parent → invalid; its child group and account cascade out.
            var result = Run(NewRef(),
                G(2, "2000", "Bad", parent: "GHOST"),
                G(3, "2100", "Child", parent: "2000"),
                A(4, "100001", "Cash", group: "2100"));

            result.Groups.Should().BeEmpty();
            result.Accounts.Should().BeEmpty();
        }
    }
}
