#nullable disable
using System.Data;
using PurchaseManagement.Application.Common.Interfaces.ITnCTemplateMaster;
using PurchaseManagement.Application.TnCTemplateMaster.Queries.GetAllTnCTemplateMaster;
using PurchaseManagement.Application.TnCTemplateMaster.Queries.GetTnCTemplateMasterAutoComplete;
using Dapper;

namespace PurchaseManagement.Infrastructure.Repositories.TncTemplateMaster
{
    public class TncTemplateMasterQueryRepository : ITnCTemplateMasterQueryRepository
    {

        private readonly IDbConnection _dbConnection;

        public TncTemplateMasterQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
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

            // ---- TOTAL (distinct master count, soft-delete aware)
            const string countSql = @"
SELECT COUNT(DISTINCT t.Id)
FROM   [Purchase].[TnCTemplateMaster] t
LEFT   JOIN [Purchase].[MiscMaster] tt ON tt.Id = t.TemplateTypeId
LEFT   JOIN [Purchase].[TnCTemplateApplicability] ta 
           ON ta.TnCTemplateMasterId = t.Id AND ta.IsDeleted = 0
LEFT   JOIN [Purchase].[MiscMaster] mm ON mm.Id = ta.ApplicabilityId
WHERE  t.IsDeleted = 0
AND   (
        @Search IS NULL OR @Search = '' OR
        t.TemplateName LIKE @Search OR
        t.TemplateCode LIKE @Search OR
        tt.Code        LIKE @Search OR
        tt.[Description] LIKE @Search OR
        mm.Code        LIKE @Search OR
        mm.[Description] LIKE @Search
      );";

            var totalCount = await _dbConnection.ExecuteScalarAsync<int>(countSql, args);

            // ---- PAGE (masters + child rows)
            // NOTE:
            //  - master columns come BEFORE the splitOn column
            //  - child columns start at ta.Id (aliased as Id to match TncApplicabilityDto.Id)
            const string pageSql = @"
SELECT 
    -- master columns (BEFORE splitOn)
    p.Id,
    p.TemplateCode,
    p.TemplateName,
    p.TemplateTypeId,
    p.TemplateTypeCode,
    p.TemplateTypeDescription,
    '' AS TermsHtml,
    p.ApprovalFlag,
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
    ta.ApplicabilityId,
    mm.Code              AS Code,              
    mm.[Description]     AS [Description]
FROM (
    SELECT 
        t.Id,
        t.TemplateCode,
        t.TemplateName,
        t.TemplateTypeId,
        tt.Code          AS TemplateTypeCode,
        tt.[Description] AS TemplateTypeDescription,
        t.TermsHtml,
        t.ApprovalFlag,
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
    LEFT   JOIN [Purchase].[MiscMaster] tt ON tt.Id = t.TemplateTypeId
    WHERE  t.IsDeleted = 0
    AND   (
            @Search IS NULL OR @Search = '' OR
            t.TemplateName LIKE @Search OR
            t.TemplateCode LIKE @Search OR
            tt.Code        LIKE @Search OR
            tt.[Description] LIKE @Search
          )
    ORDER BY t.CreatedDate DESC, t.Id DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
) p
LEFT JOIN [Purchase].[TnCTemplateApplicability] ta 
       ON ta.TnCTemplateMasterId = p.Id AND ta.IsDeleted = 0
LEFT JOIN [Purchase].[MiscMaster] mm ON mm.Id = ta.ApplicabilityId
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
                    if (child != null && child.ApplicabilityId > 0)
                    {
                        // Dapper already mapped:
                        //  child.Id                  -> TnCTemplateApplicability.Id (junction PK)
                        //  child.TnCTemplateMasterId -> FK
                        //  child.ApplicabilityId     -> MiscMaster.Id
                        //  child.Code/Description    -> from MiscMaster
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
            t.TemplateTypeId,
            tt.Code          AS TemplateTypeCode,
            tt.[Description] AS TemplateTypeDescription,
            t.TermsHtml,
            t.ApprovalFlag,
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
            ta.ApplicabilityId,
            mm.Code              AS Code,
            mm.[Description]     AS [Description]
        FROM [Purchase].[TnCTemplateMaster] t
        LEFT JOIN [Purchase].[MiscMaster] tt ON tt.Id = t.TemplateTypeId
        LEFT JOIN [Purchase].[TnCTemplateApplicability] ta 
            ON ta.TnCTemplateMasterId = t.Id AND ta.IsDeleted = 0
        LEFT JOIN [Purchase].[MiscMaster] mm ON mm.Id = ta.ApplicabilityId
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

                    if (child != null && child.ApplicabilityId > 0)
                    {
                        dto.Applicabilities.Add(child);
                    }

                    return dto;
                },
                new { Id = id },
                splitOn: "Id"
            );

            return lookup.Values.FirstOrDefault();
        }



        public async Task<bool> ExistsByTypeAndNameAsync(int templateTypeId, string templateName, int? excludeId = null, CancellationToken ct = default)
        {
            const string sql = @"
            SELECT 1
            FROM   [Purchase].[TnCTemplateMaster]
            WHERE  TemplateTypeId = @TypeId
            AND  TemplateName   = @Name
            AND  IsDeleted      = 0
            AND  (@ExcludeId IS NULL OR Id <> @ExcludeId);";

            var cmd = new CommandDefinition(
                sql,
                new { TypeId = templateTypeId, Name = templateName, ExcludeId = excludeId },
                cancellationToken: ct);

            var exists = await _dbConnection.ExecuteScalarAsync<int?>(cmd);
            return exists.HasValue;
        }
        public async Task<bool> ApplicabilitiesExistAsync(IEnumerable<int> ids, CancellationToken ct = default)
        {
            var idList = ids?.Distinct().ToArray() ?? Array.Empty<int>();
            if (idList.Length == 0) return false;

            const string sql = @"
                SELECT COUNT(*)
                FROM [Purchase].[MiscMaster]
                WHERE IsDeleted = 0
                AND MiscTypeId = 9   -- Applicability
                AND Id IN @Ids;";

            var cmd = new CommandDefinition(sql, new { Ids = idList }, cancellationToken: ct);
            var count = await _dbConnection.ExecuteScalarAsync<int>(cmd);

            return count == idList.Length;
        }

        public async Task<bool> IsUsedInTransactionsAsync(int templateId, CancellationToken ct = default)
        {
            const string sql = @"
                SELECT TOP 1 1
                FROM Purchase.Purchase.PurchaseOrder po WITH (NOLOCK)
                WHERE po.TnCTemplateId = @Id
                UNION ALL
                SELECT TOP 1 1
                FROM Sales.Sales.SalesOrder so WITH (NOLOCK)
                WHERE so.TnCTemplateId = @Id;";

            var used = await _dbConnection.ExecuteScalarAsync<int?>(sql, new { Id = templateId });
            return used.HasValue;
        }
         
         

         public async Task<List<TnCAutoCompleteDto>> GetTnCTemplateAutoCompleteAsync(  string search,   int? templateTypeId, int? applicabilityId)
        {
            var args = new
            {
                Search = string.IsNullOrWhiteSpace(search) ? null : $"%{search}%",
                TemplateTypeId = templateTypeId,
                ApplicabilityId = applicabilityId
                
            };

            const string sql = @"
            SELECT 
                t.Id                       AS Id,
                t.TemplateName             AS TemplateName,
                t.TemplateCode             AS Code
            FROM   Purchase.Purchase.TnCTemplateMaster t
                LEFT JOIN Purchase.Purchase.TnCTemplateApplicability a
                        ON a.TnCTemplateMasterId = t.Id
                        AND a.IsDeleted = 0
            WHERE  t.IsDeleted = 0
            AND  t.IsActive  = 1
            AND (@TemplateTypeId  IS NULL OR t.TemplateTypeId  = @TemplateTypeId)
            AND (@ApplicabilityId IS NULL OR a.ApplicabilityId = @ApplicabilityId)
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
