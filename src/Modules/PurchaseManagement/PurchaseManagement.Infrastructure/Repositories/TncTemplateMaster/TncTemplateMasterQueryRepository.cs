#nullable disable
using System.Data;
using Contracts.Dtos.Lookups.Finance;
using Contracts.Interfaces.Lookups.Finance;
using Contracts.Interfaces.Lookups.Users;
using PurchaseManagement.Application.Common.Interfaces.ITnCTemplateMaster;
using PurchaseManagement.Application.TnCTemplateMaster.Queries.GetAllTnCTemplateMaster;
using PurchaseManagement.Application.TnCTemplateMaster.Queries.GetTnCTemplateMasterAutoComplete;
using Dapper;

namespace PurchaseManagement.Infrastructure.Repositories.TncTemplateMaster
{
    public class TncTemplateMasterQueryRepository : ITnCTemplateMasterQueryRepository
    {

        private readonly IDbConnection _dbConnection;
        private readonly IModuleLookup _moduleLookup;
        private readonly ITransactionTypeLookup _transactionTypeLookup;

        public TncTemplateMasterQueryRepository(
            IDbConnection dbConnection,
            IModuleLookup moduleLookup,
            ITransactionTypeLookup transactionTypeLookup)
        {
            _dbConnection = dbConnection;
            _moduleLookup = moduleLookup;
            _transactionTypeLookup = transactionTypeLookup;
        }

        public async Task<(List<TncTemplateMasterDto>, int)> GetAllTncTemplateAsync(
            int pageNumber, int pageSize, string searchTerm)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            var args = new
            {
                Search = string.IsNullOrWhiteSpace(searchTerm) ? null : $"%{searchTerm!.Trim()}%",
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            // ---- TOTAL (distinct master count, soft-delete aware).
            // Cross-module name search is not possible (no JOIN to AppData.Modules / Finance.*),
            // so search is limited to the master's own columns.
            const string countSql = @"
SELECT COUNT(*)
FROM   [Purchase].[TnCTemplateMaster] t
WHERE  t.IsDeleted = 0
AND   (
        @Search IS NULL OR @Search = '' OR
        t.TemplateName LIKE @Search OR
        t.TemplateCode LIKE @Search
      );";

            var totalCount = await _dbConnection.ExecuteScalarAsync<int>(countSql, args);

            // ---- PAGE (masters + child rows). No cross-module JOINs; names filled via lookups.
            const string pageSql = @"
SELECT
    -- master columns (BEFORE splitOn)
    p.Id,
    p.TemplateCode,
    p.TemplateName,
    p.ModuleId,
    '' AS TermsHtml,
    p.IsActive,
    p.IsDeleted,
    p.CreatedBy,
    p.CreatedDate,
    p.CreatedByName,
    p.CreatedIP,
    p.ModifiedBy,
    p.ModifiedDate,
    p.ModifiedByName,
    p.ModifiedIP,

    -- child columns (AFTER splitOn)
    ta.Id                AS Id,
    ta.TnCTemplateMasterId,
    ta.TransactionTypeId
FROM (
    SELECT
        t.Id,
        t.TemplateCode,
        t.TemplateName,
        t.ModuleId,
        t.TermsHtml,
        t.IsActive,
        t.IsDeleted,
        t.CreatedBy,
        t.CreatedDate,
        t.CreatedByName,
        t.CreatedIP,
        t.ModifiedBy,
        t.ModifiedDate,
        t.ModifiedByName,
        t.ModifiedIP
    FROM   [Purchase].[TnCTemplateMaster] t
    WHERE  t.IsDeleted = 0
    AND   (
            @Search IS NULL OR @Search = '' OR
            t.TemplateName LIKE @Search OR
            t.TemplateCode LIKE @Search
          )
    ORDER BY t.CreatedDate DESC, t.Id DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
) p
LEFT JOIN [Purchase].[TnCTemplateApplicability] ta
       ON ta.TnCTemplateMasterId = p.Id AND ta.IsDeleted = 0
ORDER BY p.CreatedDate DESC, p.Id DESC, ta.Id ASC;";

            var byId = new Dictionary<int, TncTemplateMasterDto>();

            // Multi-mapping: master + child row
            await _dbConnection.QueryAsync<TncTemplateMasterDto, TncApplicabilityDto, TncTemplateMasterDto>(
                pageSql,
                (master, child) =>
                {
                    if (!byId.TryGetValue(master.Id, out var dto))
                    {
                        dto = master;
                        dto.Applicabilities = new List<TncApplicabilityDto>();
                        byId.Add(dto.Id, dto);
                    }

                    // If a child row exists (LEFT JOIN can be null)
                    if (child != null && child.TransactionTypeId > 0)
                    {
                        dto.Applicabilities.Add(child);
                    }
                    return dto;
                },
                param: args,
                splitOn: "Id"   // start mapping child at the 'Id' column of ta (aliased as Id)
            );

            // Materialize in stable order
            var items = byId.Values
                            .OrderByDescending(x => x.CreatedDate ?? DateTimeOffset.MinValue)
                            .ThenByDescending(x => x.Id)
                            .ToList();

            await PopulateLookupNamesAsync(items);

            return (items, totalCount);
        }

        public async Task<TncTemplateMasterDto> GetByIdAsync(int id)
        {
            const string sql = @"
        SELECT
            -- master columns (before splitOn)
            t.Id,
            t.TemplateCode,
            t.TemplateName,
            t.ModuleId,
            t.TermsHtml,
            t.IsActive,
            t.IsDeleted,
            t.CreatedBy,
            t.CreatedDate,
            t.CreatedByName,
            t.CreatedIP,
            t.ModifiedBy,
            t.ModifiedDate,
            t.ModifiedByName,
            t.ModifiedIP,

            -- child columns (after splitOn)
            ta.Id                AS Id,
            ta.TnCTemplateMasterId,
            ta.TransactionTypeId
        FROM [Purchase].[TnCTemplateMaster] t
        LEFT JOIN [Purchase].[TnCTemplateApplicability] ta
            ON ta.TnCTemplateMasterId = t.Id AND ta.IsDeleted = 0
        WHERE t.Id = @Id AND t.IsDeleted = 0;";

            var lookup = new Dictionary<int, TncTemplateMasterDto>();

            await _dbConnection.QueryAsync<TncTemplateMasterDto, TncApplicabilityDto, TncTemplateMasterDto>(
                sql,
                (master, child) =>
                {
                    if (!lookup.TryGetValue(master.Id, out var dto))
                    {
                        dto = master;
                        dto.Applicabilities = new List<TncApplicabilityDto>();
                        lookup.Add(dto.Id, dto);
                    }

                    if (child != null && child.TransactionTypeId > 0)
                    {
                        dto.Applicabilities.Add(child);
                    }

                    return dto;
                },
                new { Id = id },
                splitOn: "Id"
            );

            var result = lookup.Values.FirstOrDefault();
            if (result != null)
                await PopulateLookupNamesAsync(new[] { result });

            return result;
        }

        // Fills ModuleName (from IModuleLookup) and TransactionType TypeName/ShortName
        // (from ITransactionTypeLookup) — cross-module data fetched via lookups, never JOINs.
        private async Task PopulateLookupNamesAsync(IReadOnlyCollection<TncTemplateMasterDto> items)
        {
            if (items == null || items.Count == 0) return;

            var modules = await _moduleLookup.GetAllModuleAsync();
            var moduleMap = modules
                .GroupBy(m => m.ModuleId)
                .ToDictionary(g => g.Key, g => g.First().ModuleName);

            var txnIds = items
                .Where(x => x.Applicabilities != null)
                .SelectMany(x => x.Applicabilities)
                .Select(a => a.TransactionTypeId)
                .Where(idValue => idValue > 0)
                .Distinct()
                .ToList();

            var txnMap = new Dictionary<int, TransactionTypeLookupDto>();
            if (txnIds.Count > 0)
            {
                var txns = await _transactionTypeLookup.GetByIdsAsync(txnIds);
                txnMap = txns
                    .GroupBy(t => t.Id)
                    .ToDictionary(g => g.Key, g => g.First());
            }

            foreach (var item in items)
            {
                if (moduleMap.TryGetValue(item.ModuleId, out var moduleName))
                    item.ModuleName = moduleName;

                if (item.Applicabilities == null) continue;
                foreach (var app in item.Applicabilities)
                {
                    if (txnMap.TryGetValue(app.TransactionTypeId, out var txn))
                    {
                        app.TypeName = txn.TypeName;
                        app.ShortName = txn.ShortName;
                    }
                }
            }
        }

        public async Task<bool> ExistsByModuleAndNameAsync(int moduleId, string templateName, int? excludeId = null, CancellationToken ct = default)
        {
            const string sql = @"
            SELECT 1
            FROM   [Purchase].[TnCTemplateMaster]
            WHERE  ModuleId = @ModuleId
            AND  TemplateName   = @Name
            AND  IsDeleted      = 0
            AND  (@ExcludeId IS NULL OR Id <> @ExcludeId);";

            var cmd = new CommandDefinition(
                sql,
                new { ModuleId = moduleId, Name = templateName, ExcludeId = excludeId },
                cancellationToken: ct);

            var exists = await _dbConnection.ExecuteScalarAsync<int?>(cmd);
            return exists.HasValue;
        }

        // Validates transaction type ids via the cross-module lookup (no JOIN to Finance schema).
        public async Task<bool> TransactionTypesExistAsync(IEnumerable<int> ids, CancellationToken ct = default)
        {
            var idList = ids?.Distinct().Where(x => x > 0).ToArray() ?? Array.Empty<int>();
            if (idList.Length == 0) return false;

            var found = await _transactionTypeLookup.GetByIdsAsync(idList);
            var foundIds = found.Select(t => t.Id).Distinct().ToHashSet();

            return idList.All(id => foundIds.Contains(id));
        }

        public async Task<bool> IsUsedInTransactionsAsync(int templateId, CancellationToken ct = default)
        {
            const string sql = @"
                SELECT TOP 1 1
                FROM Purchase.PurchaseOrder po WITH (NOLOCK)
                WHERE po.TnCTemplateId = @Id
                UNION ALL
                SELECT TOP 1 1
                FROM Sales.SalesOrder so WITH (NOLOCK)
                WHERE so.TnCTemplateId = @Id;";

            var used = await _dbConnection.ExecuteScalarAsync<int?>(sql, new { Id = templateId });
            return used.HasValue;
        }



         public async Task<List<TnCAutoCompleteDto>> GetTnCTemplateAutoCompleteAsync(string search, int? moduleId, int? transactionTypeId)
        {
            var args = new
            {
                Search = string.IsNullOrWhiteSpace(search) ? null : $"%{search}%",
                ModuleId = moduleId,
                TransactionTypeId = transactionTypeId

            };

            const string sql = @"
            SELECT
                t.Id                       AS Id,
                t.TemplateName             AS TemplateName,
                t.TemplateCode             AS Code
            FROM   Purchase.TnCTemplateMaster t
                LEFT JOIN Purchase.TnCTemplateApplicability a
                        ON a.TnCTemplateMasterId = t.Id
                        AND a.IsDeleted = 0
            WHERE  t.IsDeleted = 0
            AND  t.IsActive  = 1
            AND (@ModuleId         IS NULL OR t.ModuleId          = @ModuleId)
            AND (@TransactionTypeId IS NULL OR a.TransactionTypeId = @TransactionTypeId)
            AND (
                    @Search IS NULL
                    OR t.TemplateName LIKE @Search
                    OR t.TemplateCode LIKE @Search
                )
            GROUP BY t.Id, t.TemplateName, t.TemplateCode
            ORDER BY t.TemplateName;";

            var rows = await _dbConnection.QueryAsync<TnCAutoCompleteDto>(sql, args);
            return rows.ToList();
        }

        public async Task<bool> SoftDeleteValidationAsync(int id)
        {
            const string query = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM [Purchase].[TnCTemplateApplicability]
                    WHERE TnCTemplateMasterId = @id AND IsDeleted = 0
                ) THEN 1 ELSE 0 END;";

            var result = await _dbConnection.QueryFirstOrDefaultAsync<int>(query, new { id });
            return result == 1;
        }

        public async Task<bool> IsTnCTemplateLinkedAsync(int id)
        {
            const string query = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM [Purchase].[TnCTemplateApplicability]
                    WHERE TnCTemplateMasterId = @id AND IsDeleted = 0 AND IsActive = 1
                ) THEN 1 ELSE 0 END;";

            var result = await _dbConnection.QueryFirstOrDefaultAsync<int>(query, new { id });
            return result == 1;
        }

    }
}
