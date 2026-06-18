using System.Text.RegularExpressions;
using FinanceManagement.Application.Common.Interfaces.IGlAccountImport;
using FinanceManagement.Application.GlAccountImport.Dto;
// NOTE: AccountGroupEntity alias avoids the clash with the FinanceManagement.Application.AccountGroup
// feature namespace, which otherwise shadows the Domain entity of the same name.
using AccountGroupEntity = FinanceManagement.Domain.Entities.AccountGroup;

namespace FinanceManagement.Application.GlAccountImport.Services
{
    /// <summary>
    /// Two-pass COA validator. Pass 1 builds & validates the group hierarchy (parent resolution,
    /// circular-reference/loop detection via DFS, max depth, statutory-head rules); pass 2 validates
    /// accounts (mandatory fields, code format reused from FR-001, duplicate codes, leaf-group
    /// attachment, FK code resolution). Pure in-memory — collects every failure, commits nothing.
    /// Rows whose code already exists in the DB are treated as existing (no-op) so a full export
    /// re-imports cleanly (AC5) and incremental imports don't disturb existing data.
    /// </summary>
    public sealed class GlAccountImportValidator : IGlAccountImportValidator
    {
        // Column sizes mirror the EF configuration (Finance.AccountGroup / Finance.GlAccountMaster).
        private const int GroupCodeMax = 50;
        private const int GroupNameMax = 150;
        private const int AccountCodeMax = 20;
        private const int AccountNameMax = 200;
        private const int DescriptionMax = 500;

        private static readonly Regex DigitsOnly = new("^[0-9]+$", RegexOptions.Compiled);
        private const int Cycle = -2;
        private const int Unresolved = -1;

        public GlAccountImportValidationResult Validate(
            IReadOnlyList<GlAccountImportRowDto> rows,
            IReadOnlyList<GlAccountImportErrorDto> parseErrors,
            GlAccountImportReferenceData refData)
        {
            var result = new GlAccountImportValidationResult();
            result.Errors.AddRange(parseErrors);

            var groupRows = new List<GlAccountImportRowDto>();
            var accountRows = new List<GlAccountImportRowDto>();
            foreach (var row in rows)
            {
                var type = (row.RecordType ?? string.Empty).Trim().ToUpperInvariant();
                if (type == GlAccountImportColumns.RecordTypeGroup) groupRows.Add(row);
                else if (type == GlAccountImportColumns.RecordTypeAccount) accountRows.Add(row);
                else
                    result.Errors.Add(Err(row.RowNumber, row.RecordType, "RecordType", row.RecordType,
                        "RecordType must be 'GROUP' or 'ACCOUNT'."));
            }

            result.TotalRows = rows.Count;
            result.GroupRows = groupRows.Count;
            result.AccountRows = accountRows.Count;

            var dbGroups = refData.GroupsByCode; // key = upper(GroupCode)

            // ── Pass 1: groups ────────────────────────────────────────────────
            var newByCode = new Dictionary<string, NewGroup>();
            var dupGroupCodes = new HashSet<string>();

            foreach (var row in groupRows)
            {
                var codeRaw = row.GroupCode;
                if (string.IsNullOrWhiteSpace(codeRaw))
                {
                    result.Errors.Add(Err(row.RowNumber, "GROUP", "GroupCode", codeRaw, "GroupCode is required."));
                    continue;
                }

                var key = Key(codeRaw);

                // Already in DB → existing group, not re-created (keeps existing untouched / AC5).
                if (dbGroups.ContainsKey(key)) continue;

                if (newByCode.ContainsKey(key) || dupGroupCodes.Contains(key))
                {
                    dupGroupCodes.Add(key);
                    result.Errors.Add(Err(row.RowNumber, "GROUP", "GroupCode", codeRaw,
                        $"Duplicate GroupCode '{codeRaw}' in the file."));
                    continue;
                }

                bool ok = true;
                if (codeRaw.Length > GroupCodeMax)
                {
                    ok = false;
                    result.Errors.Add(Err(row.RowNumber, "GROUP", "GroupCode", codeRaw,
                        $"GroupCode cannot be longer than {GroupCodeMax} characters."));
                }
                if (string.IsNullOrWhiteSpace(row.GroupName))
                {
                    ok = false;
                    result.Errors.Add(Err(row.RowNumber, "GROUP", "GroupName", row.GroupName, "GroupName is required."));
                }
                else if (row.GroupName.Length > GroupNameMax)
                {
                    ok = false;
                    result.Errors.Add(Err(row.RowNumber, "GROUP", "GroupName", row.GroupName,
                        $"GroupName cannot be longer than {GroupNameMax} characters."));
                }

                var parentKey = string.IsNullOrWhiteSpace(row.ParentGroupCode) ? null : Key(row.ParentGroupCode);
                int? accountTypeId = null;

                if (parentKey == null)
                {
                    // Level-1 root → AccountType (statutory head) required.
                    if (string.IsNullOrWhiteSpace(row.AccountType))
                    {
                        ok = false;
                        result.Errors.Add(Err(row.RowNumber, "GROUP", "AccountType", row.AccountType,
                            "AccountType is required for a Level-1 (root) group."));
                    }
                    else if (!refData.AccountTypesByName.TryGetValue(Key(row.AccountType), out var at))
                    {
                        ok = false;
                        result.Errors.Add(Err(row.RowNumber, "GROUP", "AccountType", row.AccountType,
                            $"AccountType '{row.AccountType}' does not exist."));
                    }
                    else accountTypeId = at.Id;
                }
                else
                {
                    // Sub-level → AccountType must NOT be set; parent must resolve.
                    if (!string.IsNullOrWhiteSpace(row.AccountType))
                    {
                        ok = false;
                        result.Errors.Add(Err(row.RowNumber, "GROUP", "AccountType", row.AccountType,
                            "AccountType is only allowed on a Level-1 (root) group."));
                    }
                    if (parentKey == key)
                    {
                        ok = false;
                        result.Errors.Add(Err(row.RowNumber, "GROUP", "ParentGroupCode", row.ParentGroupCode,
                            "A group cannot be its own parent (hierarchy loop)."));
                    }
                }

                if (!ok) continue;

                newByCode[key] = new NewGroup
                {
                    Row = row,
                    Code = codeRaw,
                    Name = row.GroupName!,
                    ParentKey = parentKey == key ? null : parentKey,
                    ParentCodeRaw = row.ParentGroupCode,
                    AccountTypeId = accountTypeId,
                    SortOrder = ParseIntOrZero(row.SortOrder)
                };
            }

            // Parent existence + level/cycle resolution for the surviving new groups.
            var levelCache = new Dictionary<string, int>();
            foreach (var ng in newByCode.Values)
            {
                if (ng.ParentKey != null && !dbGroups.ContainsKey(ng.ParentKey) && !newByCode.ContainsKey(ng.ParentKey))
                {
                    ng.Invalid = true;
                    result.Errors.Add(Err(ng.Row.RowNumber, "GROUP", "ParentGroupCode", ng.ParentCodeRaw,
                        $"Parent group '{ng.ParentCodeRaw}' does not exist."));
                }
            }

            foreach (var ng in newByCode.Values.Where(g => !g.Invalid))
            {
                int level = ResolveLevel(ng.Code, newByCode, dbGroups, levelCache, new HashSet<string>());
                if (level == Cycle)
                {
                    ng.Invalid = true;
                    result.Errors.Add(Err(ng.Row.RowNumber, "GROUP", "ParentGroupCode", ng.ParentCodeRaw,
                        "Circular parent reference (hierarchy loop) detected."));
                }
                else if (level == Unresolved)
                {
                    ng.Invalid = true; // parent chain unresolved (errored upstream)
                }
                else if (level > AccountGroupEntity.DefaultMaxDepth)
                {
                    ng.Invalid = true;
                    result.Errors.Add(Err(ng.Row.RowNumber, "GROUP", "ParentGroupCode", ng.ParentCodeRaw,
                        $"Group exceeds the maximum hierarchy depth of {AccountGroupEntity.DefaultMaxDepth} levels."));
                }
                else ng.Level = level;
            }

            // Cascade: drop a still-valid group whose new-group parent was invalidated.
            bool changed = true;
            while (changed)
            {
                changed = false;
                foreach (var ng in newByCode.Values.Where(g => !g.Invalid))
                {
                    if (ng.ParentKey != null && newByCode.TryGetValue(ng.ParentKey, out var parent) && parent.Invalid)
                    {
                        ng.Invalid = true;
                        changed = true;
                        result.Errors.Add(Err(ng.Row.RowNumber, "GROUP", "ParentGroupCode", ng.ParentCodeRaw,
                            $"Parent group '{ng.ParentCodeRaw}' could not be imported."));
                    }
                }
            }

            var plannedGroups = newByCode.Values.Where(g => !g.Invalid).ToList();

            // Codes that will exist after the run = DB groups ∪ valid new groups.
            var willExist = new HashSet<string>(dbGroups.Keys);
            foreach (var ng in plannedGroups) willExist.Add(Key(ng.Code));

            // Any group referenced as a parent (DB or new) is non-leaf and cannot hold accounts.
            var parentCodes = new HashSet<string>();
            foreach (var ng in plannedGroups)
                if (ng.ParentKey != null && willExist.Contains(ng.ParentKey)) parentCodes.Add(ng.ParentKey);

            // Emit planned groups ordered parent-before-child (level ascending).
            foreach (var ng in plannedGroups.OrderBy(g => g.Level).ThenBy(g => g.Code, StringComparer.OrdinalIgnoreCase))
            {
                int? existingParentId = null;
                if (ng.ParentKey != null && dbGroups.TryGetValue(ng.ParentKey, out var dbParent))
                    existingParentId = dbParent.Id;

                result.Groups.Add(new PlannedAccountGroup
                {
                    RowNumber = ng.Row.RowNumber,
                    GroupCode = ng.Code,
                    GroupName = ng.Name,
                    ParentGroupCode = ng.ParentCodeRaw,
                    ExistingParentId = existingParentId,
                    AccountTypeId = ng.AccountTypeId,
                    SortOrder = ng.SortOrder,
                    Level = ng.Level
                });
            }

            // ── Pass 2: accounts ──────────────────────────────────────────────
            var seenAccountCodes = new HashSet<string>();
            var seenAccountNames = new HashSet<string>();

            foreach (var row in accountRows)
            {
                var codeRaw = row.AccountCode;
                if (string.IsNullOrWhiteSpace(codeRaw))
                {
                    result.Errors.Add(Err(row.RowNumber, "ACCOUNT", "AccountCode", codeRaw, "AccountCode is required."));
                    continue;
                }

                var codeKey = Key(codeRaw);

                // Already in DB → existing account, left untouched (no-op, no error) — AC5.
                if (refData.ExistingAccountCodes.Contains(codeKey)) continue;

                bool ok = true;

                if (!seenAccountCodes.Add(codeKey))
                {
                    ok = false;
                    result.Errors.Add(Err(row.RowNumber, "ACCOUNT", "AccountCode", codeRaw,
                        $"Duplicate AccountCode '{codeRaw}' in the file."));
                }
                if (codeRaw.Length > AccountCodeMax)
                {
                    ok = false;
                    result.Errors.Add(Err(row.RowNumber, "ACCOUNT", "AccountCode", codeRaw,
                        $"AccountCode cannot be longer than {AccountCodeMax} characters."));
                }
                else if (!DigitsOnly.IsMatch(codeRaw))
                {
                    ok = false;
                    result.Errors.Add(Err(row.RowNumber, "ACCOUNT", "AccountCode", codeRaw,
                        "AccountCode must contain digits only."));
                }

                if (string.IsNullOrWhiteSpace(row.AccountName))
                {
                    ok = false;
                    result.Errors.Add(Err(row.RowNumber, "ACCOUNT", "AccountName", row.AccountName, "AccountName is required."));
                }
                else
                {
                    if (row.AccountName.Length > AccountNameMax)
                    {
                        ok = false;
                        result.Errors.Add(Err(row.RowNumber, "ACCOUNT", "AccountName", row.AccountName,
                            $"AccountName cannot be longer than {AccountNameMax} characters."));
                    }
                    var nameKey = Key(row.AccountName);
                    if (!seenAccountNames.Add(nameKey))
                    {
                        ok = false;
                        result.Errors.Add(Err(row.RowNumber, "ACCOUNT", "AccountName", row.AccountName,
                            $"Duplicate AccountName '{row.AccountName}' in the file."));
                    }
                    else if (refData.ExistingAccountNames.Contains(nameKey))
                    {
                        ok = false;
                        result.Errors.Add(Err(row.RowNumber, "ACCOUNT", "AccountName", row.AccountName,
                            $"AccountName '{row.AccountName}' already exists."));
                    }
                }

                if (!string.IsNullOrWhiteSpace(row.Description) && row.Description.Length > DescriptionMax)
                {
                    ok = false;
                    result.Errors.Add(Err(row.RowNumber, "ACCOUNT", "Description", row.Description,
                        $"Description cannot be longer than {DescriptionMax} characters."));
                }

                // Owning group must exist & be a leaf.
                var groupKey = string.IsNullOrWhiteSpace(row.AccountGroupCode) ? null : Key(row.AccountGroupCode);
                int? existingGroupId = null;
                int? rootAccountTypeId = null;
                if (groupKey == null)
                {
                    ok = false;
                    result.Errors.Add(Err(row.RowNumber, "ACCOUNT", "AccountGroupCode", row.AccountGroupCode,
                        "AccountGroupCode is required."));
                }
                else if (!willExist.Contains(groupKey))
                {
                    ok = false;
                    result.Errors.Add(Err(row.RowNumber, "ACCOUNT", "AccountGroupCode", row.AccountGroupCode,
                        $"Account group '{row.AccountGroupCode}' does not exist."));
                }
                else if (parentCodes.Contains(groupKey))
                {
                    ok = false;
                    result.Errors.Add(Err(row.RowNumber, "ACCOUNT", "AccountGroupCode", row.AccountGroupCode,
                        "Accounts can attach only to a leaf group (this group has sub-groups)."));
                }
                else if (dbGroups.TryGetValue(groupKey, out var dbg) && !dbg.IsLeaf)
                {
                    ok = false;
                    result.Errors.Add(Err(row.RowNumber, "ACCOUNT", "AccountGroupCode", row.AccountGroupCode,
                        "Accounts can attach only to a leaf group (this group has sub-groups)."));
                }
                else
                {
                    if (dbGroups.TryGetValue(groupKey, out var dbGroup))
                    {
                        existingGroupId = dbGroup.Id;
                        rootAccountTypeId = dbGroup.RootAccountTypeId;
                    }
                    else
                    {
                        rootAccountTypeId = ResolveRootAccountType(groupKey, newByCode, dbGroups);
                    }
                }

                // FK code resolution.
                int normalBalanceId = ResolveCode(row.RowNumber, "NormalBalance", row.NormalBalance,
                    refData.NormalBalanceByCode, result, ref ok);
                int currencyId = ResolveCode(row.RowNumber, "Currency", row.Currency,
                    refData.CurrencyByCode, result, ref ok);
                int subLedgerId = ResolveCode(row.RowNumber, "SubLedgerType", row.SubLedgerType,
                    refData.SubLedgerTypeByCode, result, ref ok);

                // Account-code length/prefix against the group's statutory head (FR-001).
                if (rootAccountTypeId.HasValue && refData.AccountTypesById.TryGetValue(rootAccountTypeId.Value, out var fmt))
                {
                    if (DigitsOnly.IsMatch(codeRaw))
                    {
                        if (fmt.AccountCodeLength > 0 && codeRaw.Length != fmt.AccountCodeLength)
                        {
                            ok = false;
                            result.Errors.Add(Err(row.RowNumber, "ACCOUNT", "AccountCode", codeRaw,
                                $"AccountCode must be exactly {fmt.AccountCodeLength} digits for account type '{fmt.AccountTypeName}'."));
                        }
                        if (!string.IsNullOrEmpty(fmt.StartCode) && !codeRaw.StartsWith(fmt.StartCode))
                        {
                            ok = false;
                            result.Errors.Add(Err(row.RowNumber, "ACCOUNT", "AccountCode", codeRaw,
                                $"AccountCode must start with '{fmt.StartCode}' for account type '{fmt.AccountTypeName}'."));
                        }
                    }
                }

                var boolCcm = ParseBool(row.RowNumber, "IsCostCentreMandatory", row.IsCostCentreMandatory, result, ref ok);
                var boolTax = ParseBool(row.RowNumber, "IsTaxRelevant", row.IsTaxRelevant, result, ref ok);
                var boolIc = ParseBool(row.RowNumber, "IsInterCompany", row.IsInterCompany, result, ref ok);
                var boolRec = ParseBool(row.RowNumber, "IsReconciliationRequired", row.IsReconciliationRequired, result, ref ok);

                if (!ok || !rootAccountTypeId.HasValue) continue;

                result.Accounts.Add(new PlannedGlAccount
                {
                    RowNumber = row.RowNumber,
                    AccountCode = codeRaw,
                    AccountName = row.AccountName!,
                    Description = row.Description,
                    AccountGroupCode = row.AccountGroupCode!,
                    ExistingAccountGroupId = existingGroupId,
                    AccountTypeId = rootAccountTypeId.Value,
                    NormalBalanceId = normalBalanceId,
                    CurrencyTypeId = currencyId,
                    SubLedgerTypeId = subLedgerId,
                    IsCostCentreMandatory = boolCcm,
                    IsTaxRelevant = boolTax,
                    IsInterCompany = boolIc,
                    IsReconciliationRequired = boolRec
                });
            }

            return result;
        }

        // Walks the parent chain to compute Level; detects cycles via the recursion stack.
        private static int ResolveLevel(
            string code,
            Dictionary<string, NewGroup> newByCode,
            Dictionary<string, ExistingGroupRef> dbGroups,
            Dictionary<string, int> cache,
            HashSet<string> stack)
        {
            if (dbGroups.TryGetValue(code, out var db)) return db.Level;
            if (!newByCode.TryGetValue(code, out var ng)) return Unresolved;
            if (cache.TryGetValue(code, out var done)) return done;
            if (!stack.Add(code)) return Cycle;

            int level;
            if (ng.ParentKey == null) level = 1;
            else
            {
                int parentLevel = ResolveLevel(ng.ParentKey, newByCode, dbGroups, cache, stack);
                level = parentLevel switch
                {
                    Cycle => Cycle,
                    Unresolved => Unresolved,
                    _ => parentLevel + 1
                };
            }

            stack.Remove(code);
            cache[code] = level;
            return level;
        }

        // Statutory head of a group's Level-1 ancestor (mixed new/DB chain).
        private static int? ResolveRootAccountType(
            string code,
            Dictionary<string, NewGroup> newByCode,
            Dictionary<string, ExistingGroupRef> dbGroups)
        {
            var guard = 0;
            while (guard++ <= AccountGroupEntity.DefaultMaxDepth)
            {
                if (dbGroups.TryGetValue(code, out var db)) return db.RootAccountTypeId;
                if (!newByCode.TryGetValue(code, out var ng)) return null;
                if (ng.ParentKey == null) return ng.AccountTypeId;
                code = ng.ParentKey;
            }
            return null;
        }

        private static int ResolveCode(int rowNumber, string column, string? value,
            Dictionary<string, int> map, GlAccountImportValidationResult result, ref bool ok)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                ok = false;
                result.Errors.Add(Err(rowNumber, "ACCOUNT", column, value, $"{column} is required."));
                return 0;
            }
            if (!map.TryGetValue(Key(value), out var id))
            {
                ok = false;
                result.Errors.Add(Err(rowNumber, "ACCOUNT", column, value, $"{column} '{value}' does not exist."));
                return 0;
            }
            return id;
        }

        private static bool ParseBool(int rowNumber, string column, string? value,
            GlAccountImportValidationResult result, ref bool ok)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            switch (value.Trim().ToLowerInvariant())
            {
                case "1": case "true": case "yes": case "y": return true;
                case "0": case "false": case "no": case "n": return false;
                default:
                    ok = false;
                    result.Errors.Add(Err(rowNumber, "ACCOUNT", column, value,
                        $"{column} must be a yes/no value (1/0, true/false, yes/no)."));
                    return false;
            }
        }

        private static int ParseIntOrZero(string? value) =>
            int.TryParse(value, out var n) ? n : 0;

        private static string Key(string value) => value.Trim().ToUpperInvariant();

        private static GlAccountImportErrorDto Err(int row, string? recordType, string? column, string? value, string message) =>
            new()
            {
                RowNumber = row,
                RecordType = recordType,
                ColumnName = column,
                AttemptedValue = value,
                ErrorMessage = message
            };

        private sealed class NewGroup
        {
            public GlAccountImportRowDto Row { get; set; } = null!;
            public string Code { get; set; } = null!;
            public string Name { get; set; } = null!;
            public string? ParentKey { get; set; }
            public string? ParentCodeRaw { get; set; }
            public int? AccountTypeId { get; set; }
            public int SortOrder { get; set; }
            public int Level { get; set; }
            public bool Invalid { get; set; }
        }
    }
}
