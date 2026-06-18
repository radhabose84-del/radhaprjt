using System.Data;
using Contracts.Interfaces.Lookups.Users;
using Dapper;
using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Application.ScheduleIII.Dto;

namespace FinanceManagement.Infrastructure.Repositories.ScheduleIII
{
    public class ScheduleIIIQueryRepository : IScheduleIIIQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly ICompanyLookup _companyLookup;
        private readonly IDivisionLookup _divisionLookup;

        public ScheduleIIIQueryRepository(
            IDbConnection dbConnection,
            ICompanyLookup companyLookup,
            IDivisionLookup divisionLookup)
        {
            _dbConnection = dbConnection;
            _companyLookup = companyLookup;
            _divisionLookup = divisionLookup;
        }

        public async Task<ScheduleIIIHeaderDto?> GetStructureAsync(int companyId, int divisionId)
        {
            // One header per (Company, Division).
            const string headerSql = @"
                SELECT TOP 1 h.Id, h.CompanyId, h.DivisionId,
                       h.StatusId, st.Description AS StructureStatusName,
                       h.TextileSplitEnabled, h.IsActive
                FROM [Finance].[ScheduleIIIHeader] h
                LEFT JOIN [Finance].[MiscMaster] st ON h.StatusId = st.Id AND st.IsDeleted = 0
                WHERE h.CompanyId = @CompanyId AND h.DivisionId = @DivisionId AND h.IsDeleted = 0;";

            var structure = await _dbConnection.QueryFirstOrDefaultAsync<ScheduleIIIHeaderDto>(
                headerSql, new { CompanyId = companyId, DivisionId = divisionId });

            // No structure header yet → return the full catalog (all sections + line items) so the
            // "Line items" builder can render straight from ScheduleIIISection / ScheduleIIISectionItem.
            if (structure == null)
            {
                var companyList = await _companyLookup.GetAllCompanyAsync();
                var divisionList = await _divisionLookup.GetByIdsAsync(new[] { divisionId });

                return new ScheduleIIIHeaderDto
                {
                    Id = 0,
                    CompanyId = companyId,
                    CompanyName = companyList.FirstOrDefault(c => c.CompanyId == companyId)?.CompanyName,
                    DivisionId = divisionId,
                    DivisionName = divisionList.FirstOrDefault(d => d.Id == divisionId)?.Name,
                    IsActive = true,
                    Sections = await GetCatalogSectionsAsync()
                };
            }

            // Cross-module names (no DB JOIN to other modules)
            var companies = await _companyLookup.GetAllCompanyAsync();
            structure.CompanyName = companies.FirstOrDefault(c => c.CompanyId == structure.CompanyId)?.CompanyName;

            var divisions = await _divisionLookup.GetByIdsAsync(new[] { structure.DivisionId });
            structure.DivisionName = divisions.FirstOrDefault(d => d.Id == structure.DivisionId)?.Name;

            // Included lines = ScheduleIIIDetail rows for the header, ordered by DisplayOrder within section.
            const string lineSql = @"
                SELECT d.Id AS DetailId, d.DisplayOrder, d.IsActive,
                       d.ScheduleIIISectionId AS SectionId,
                       li.Id, li.LineCode, li.LineName, li.NoteReference, li.IsSplitLine
                FROM [Finance].[ScheduleIIIDetail] d
                JOIN [Finance].[ScheduleIIISectionItem] li ON d.ScheduleIIISectionItemId = li.Id AND li.IsDeleted = 0
                WHERE d.ScheduleIIIHeaderId = @HeaderId AND d.IsDeleted = 0
                ORDER BY d.ScheduleIIISectionId, d.DisplayOrder;";

            var lines = (await _dbConnection.QueryAsync<ScheduleIIISectionItemDto>(
                lineSql, new { HeaderId = structure.Id })).ToList();

            var sections = new List<ScheduleIIISectionDto>();
            var sectionIds = lines.Select(l => l.SectionId).Distinct().ToList();
            if (sectionIds.Count > 0)
            {
                const string sectionSql = @"
                    SELECT sec.Id, sec.SectionName,
                           sec.StatementTypeId, stt.Description AS StatementTypeName,
                           sec.NatureId, nat.Description AS NatureName
                    FROM [Finance].[ScheduleIIISection] sec
                    LEFT JOIN [Finance].[MiscMaster] stt ON sec.StatementTypeId = stt.Id AND stt.IsDeleted = 0
                    LEFT JOIN [Finance].[MiscMaster] nat ON sec.NatureId       = nat.Id AND nat.IsDeleted = 0
                    WHERE sec.Id IN @SectionIds AND sec.IsDeleted = 0
                    ORDER BY sec.StatementTypeId, sec.SectionName;";

                sections = (await _dbConnection.QueryAsync<ScheduleIIISectionDto>(
                    sectionSql, new { SectionIds = sectionIds })).ToList();
            }

            // MappedCount stays 0 until US-GL02-03B ships ScheduleIIIAccountGroupMap.
            foreach (var sec in sections)
            {
                var items = lines.Where(l => l.SectionId == sec.Id).ToList();
                foreach (var li in items)
                    li.SectionName = sec.SectionName;
                sec.LineItems = items;
            }

            structure.Sections = sections;
            return structure;
        }

        // Global catalog — every section + all its line items (statement-type ordered).
        // Used as the GetStructure fallback when no header exists yet.
        private async Task<List<ScheduleIIISectionDto>> GetCatalogSectionsAsync()
        {
            const string sectionSql = @"
                SELECT sec.Id, sec.SectionName,
                       sec.StatementTypeId, stt.Description AS StatementTypeName,
                       sec.NatureId, nat.Description AS NatureName
                FROM [Finance].[ScheduleIIISection] sec
                LEFT JOIN [Finance].[MiscMaster] stt ON sec.StatementTypeId = stt.Id AND stt.IsDeleted = 0
                LEFT JOIN [Finance].[MiscMaster] nat ON sec.NatureId       = nat.Id AND nat.IsDeleted = 0
                WHERE sec.IsDeleted = 0
                ORDER BY sec.StatementTypeId, sec.SectionName;";

            var sections = (await _dbConnection.QueryAsync<ScheduleIIISectionDto>(sectionSql)).ToList();
            if (sections.Count == 0)
                return sections;

            const string lineSql = @"
                SELECT li.Id, li.SectionId, li.LineCode, li.LineName, li.NoteReference, li.IsSplitLine, li.IsActive
                FROM [Finance].[ScheduleIIISectionItem] li
                WHERE li.IsDeleted = 0
                ORDER BY li.SectionId, li.LineCode;";

            var lines = (await _dbConnection.QueryAsync<ScheduleIIISectionItemDto>(lineSql)).ToList();

            foreach (var sec in sections)
            {
                var items = lines.Where(l => l.SectionId == sec.Id).ToList();
                foreach (var li in items)
                    li.SectionName = sec.SectionName;
                sec.LineItems = items;
            }

            return sections;
        }

        public async Task<List<SubTotalFormulaOperandDto>> GetSubTotalFormulaOperandsAsync(int? subTotalId)
        {
            // Candidate operands = active P&L line items + other sub-totals (excluding the one being edited),
            // each LEFT JOINed to the editing sub-total's formula so the row shows its current +/− selection.
            // SortGroup 0 = line items first, 1 = sub-totals (matches the Edit-formula dialog order).
            const string sql = @"
                SELECT li.Id AS Id, li.LineName AS Name, 'LINEITEM' AS Kind,
                       CASE WHEN f.Id IS NULL THEN CAST(0 AS bit) ELSE CAST(1 AS bit) END AS IsSelected,
                       f.OperatorId, op.Code AS OperatorCode, f.DisplayOrder,
                       0 AS SortGroup, li.LineName AS SortKey
                FROM [Finance].[ScheduleIIISectionItem] li
                JOIN [Finance].[ScheduleIIISection] sec ON li.SectionId = sec.Id AND sec.IsDeleted = 0
                JOIN [Finance].[MiscMaster] stt ON sec.StatementTypeId = stt.Id AND stt.IsDeleted = 0 AND stt.Code = 'PL'
                LEFT JOIN [Finance].[ScheduleIIISubTotalFormula] f
                    ON f.SubTotalId = @SubTotalId AND f.IsDeleted = 0 AND f.SectionItemId = li.Id
                LEFT JOIN [Finance].[MiscMaster] op ON f.OperatorId = op.Id AND op.IsDeleted = 0
                WHERE li.IsDeleted = 0 AND li.IsActive = 1

                UNION ALL

                SELECT st.Id AS Id, st.FormulaName AS Name, 'SUBTOTAL' AS Kind,
                       CASE WHEN f.Id IS NULL THEN CAST(0 AS bit) ELSE CAST(1 AS bit) END AS IsSelected,
                       f.OperatorId, op.Code AS OperatorCode, f.DisplayOrder,
                       1 AS SortGroup, RIGHT('0000000000' + CAST(st.DisplayOrder AS varchar(10)), 10) AS SortKey
                FROM [Finance].[ScheduleIIISubTotal] st
                LEFT JOIN [Finance].[ScheduleIIISubTotalFormula] f
                    ON f.SubTotalId = @SubTotalId AND f.IsDeleted = 0 AND f.OperandSubTotalId = st.Id
                LEFT JOIN [Finance].[MiscMaster] op ON f.OperatorId = op.Id AND op.IsDeleted = 0
                WHERE st.IsDeleted = 0 AND st.IsActive = 1
                  AND (@SubTotalId IS NULL OR st.Id <> @SubTotalId)

                ORDER BY SortGroup, SortKey;";

            var result = await _dbConnection.QueryAsync<SubTotalFormulaOperandDto>(sql, new { SubTotalId = subTotalId });
            return result.ToList();
        }

        public async Task<bool> DetailNotFoundAsync(int id)
        {
            const string sql = @"SELECT COUNT(1) FROM [Finance].[ScheduleIIIDetail] WHERE Id = @Id AND IsDeleted = 0;";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        // A line appears at most once per structure (the header of CompanyId/DivisionId). Excludes self on update.
        public async Task<bool> DetailLineExistsAsync(int companyId, int divisionId, int sectionItemId, int? id = null)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM [Finance].[ScheduleIIIDetail] d
                JOIN [Finance].[ScheduleIIIHeader] h ON d.ScheduleIIIHeaderId = h.Id AND h.IsDeleted = 0
                WHERE h.CompanyId = @CompanyId AND h.DivisionId = @DivisionId
                  AND d.ScheduleIIISectionItemId = @LineId AND d.IsDeleted = 0
                  AND (@Id IS NULL OR d.Id <> @Id);";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql,
                new { CompanyId = companyId, DivisionId = divisionId, LineId = sectionItemId, Id = id });
            return count > 0;
        }

        // Display order is unique within a structure. Excludes self on update.
        public async Task<bool> DetailDisplayOrderExistsAsync(int companyId, int divisionId, int displayOrder, int? id = null)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM [Finance].[ScheduleIIIDetail] d
                JOIN [Finance].[ScheduleIIIHeader] h ON d.ScheduleIIIHeaderId = h.Id AND h.IsDeleted = 0
                WHERE h.CompanyId = @CompanyId AND h.DivisionId = @DivisionId
                  AND d.DisplayOrder = @Order AND d.IsDeleted = 0
                  AND (@Id IS NULL OR d.Id <> @Id);";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql,
                new { CompanyId = companyId, DivisionId = divisionId, Order = displayOrder, Id = id });
            return count > 0;
        }

        public async Task<(List<ActivityLogDto>, int)> GetActivityLogAsync(string? entityName, int? entityId, int pageNumber, int pageSize)
        {
            var nameClause = string.IsNullOrWhiteSpace(entityName) ? "" : "AND EntityName LIKE @EntityName";
            var idClause = entityId.HasValue ? "AND EntityId = @EntityId" : "";

            var query = $$"""
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM [Finance].[ActivityLog]
                WHERE 1 = 1 {{nameClause}} {{idClause}};

                SELECT Id, EntityName, EntityId, Action, PropertyName, OldValue, NewValue,
                       CreatedBy, CreatedByName, CreatedIP, CreatedDate, Scope, ScopeKey
                FROM [Finance].[ActivityLog]
                WHERE 1 = 1 {{nameClause}} {{idClause}}
                ORDER BY CreatedDate DESC, Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;
            """;

            var parameters = new
            {
                EntityName = string.IsNullOrWhiteSpace(entityName) ? null : $"%{entityName}%",
                EntityId = entityId,
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            var result = await _dbConnection.QueryMultipleAsync(query, parameters);
            var list = (await result.ReadAsync<ActivityLogDto>()).ToList();
            var totalCount = await result.ReadFirstAsync<int>();
            return (list, totalCount);
        }

        public async Task<Preview03BDto> Get03BPreviewAsync(int companyId, int divisionId)
        {
            const string sql = @"
                SELECT li.Id AS LineItemId, li.LineName, sec.SectionName, li.NoteReference,
                       stt.Code AS StatementTypeCode
                FROM [Finance].[ScheduleIIIDetail] d
                JOIN [Finance].[ScheduleIIIHeader] h ON d.ScheduleIIIHeaderId = h.Id AND h.IsDeleted = 0
                JOIN [Finance].[ScheduleIIISectionItem] li ON d.ScheduleIIISectionItemId = li.Id AND li.IsDeleted = 0
                JOIN [Finance].[ScheduleIIISection] sec ON d.ScheduleIIISectionId = sec.Id AND sec.IsDeleted = 0
                JOIN [Finance].[MiscMaster] stt ON sec.StatementTypeId = stt.Id AND stt.IsDeleted = 0
                WHERE h.CompanyId = @CompanyId AND h.DivisionId = @DivisionId AND d.IsDeleted = 0 AND li.IsActive = 1
                ORDER BY d.DisplayOrder;";

            var rows = (await _dbConnection.QueryAsync<Preview03BRow>(
                sql, new { CompanyId = companyId, DivisionId = divisionId })).ToList();

            var preview = new Preview03BDto();
            foreach (var r in rows)
            {
                var item = new Preview03BItemDto
                {
                    LineItemId = r.LineItemId,
                    LineName = r.LineName,
                    SectionName = r.SectionName,
                    NoteReference = r.NoteReference
                };
                if (string.Equals(r.StatementTypeCode, "BS", StringComparison.OrdinalIgnoreCase))
                    preview.BalanceSheetLeaves.Add(item);
                else if (string.Equals(r.StatementTypeCode, "PL", StringComparison.OrdinalIgnoreCase))
                    preview.ProfitAndLossLeaves.Add(item);
            }
            return preview;
        }

        public async Task<List<ScheduleIIISubTotalDto>> GetSubTotalsAsync()
        {
            const string subTotalSql = @"
                SELECT st.Id, st.FormulaName,
                       st.FormulaExpression, st.IncludeOtherIncome, st.DisplayOrder, st.IsActive
                FROM [Finance].[ScheduleIIISubTotal] st
                WHERE st.IsDeleted = 0
                ORDER BY st.DisplayOrder;";

            var subTotals = (await _dbConnection.QueryAsync<ScheduleIIISubTotalDto>(subTotalSql)).ToList();

            if (subTotals.Count == 0)
                return subTotals;

            const string formulaSql = @"
                SELECT f.Id, f.SubTotalId,
                       f.OperandTypeId, ot.Description AS OperandTypeName, ot.Code AS OperandTypeCode,
                       f.SectionItemId, f.OperandSubTotalId,
                       f.OperatorId, op.Description AS OperatorName,
                       f.DisplayOrder
                FROM [Finance].[ScheduleIIISubTotalFormula] f
                LEFT JOIN [Finance].[MiscMaster] ot ON f.OperandTypeId = ot.Id AND ot.IsDeleted = 0
                LEFT JOIN [Finance].[MiscMaster] op ON f.OperatorId    = op.Id AND op.IsDeleted = 0
                WHERE f.SubTotalId IN @SubTotalIds AND f.IsDeleted = 0
                ORDER BY f.DisplayOrder;";

            var formulas = (await _dbConnection.QueryAsync<FormulaRow>(
                formulaSql, new { SubTotalIds = subTotals.Select(s => s.Id).ToArray() })).ToList();

            // An operand is either a line item (SectionItemId) or another sub-total (OperandSubTotalId) — resolve its name.
            var lineRefIds = formulas.Where(f => f.SectionItemId.HasValue).Select(f => f.SectionItemId!.Value).Distinct().ToArray();
            var lineNames = lineRefIds.Length == 0
                ? new Dictionary<int, string?>()
                : (await _dbConnection.QueryAsync<(int Id, string? LineName)>(
                    "SELECT Id, LineName FROM [Finance].[ScheduleIIISectionItem] WHERE Id IN @Ids AND IsDeleted = 0",
                    new { Ids = lineRefIds })).ToDictionary(x => x.Id, x => x.LineName);

            var subRefIds = formulas.Where(f => f.OperandSubTotalId.HasValue).Select(f => f.OperandSubTotalId!.Value).Distinct().ToArray();
            var subNames = subRefIds.Length == 0
                ? new Dictionary<int, string?>()
                : (await _dbConnection.QueryAsync<(int Id, string? FormulaName)>(
                    "SELECT Id, FormulaName FROM [Finance].[ScheduleIIISubTotal] WHERE Id IN @Ids AND IsDeleted = 0",
                    new { Ids = subRefIds })).ToDictionary(x => x.Id, x => x.FormulaName);

            foreach (var f in formulas)
            {
                if (f.SectionItemId.HasValue && lineNames.TryGetValue(f.SectionItemId.Value, out var ln))
                    f.OperandName = ln;
                else if (f.OperandSubTotalId.HasValue && subNames.TryGetValue(f.OperandSubTotalId.Value, out var sn))
                    f.OperandName = sn;
                else
                    f.OperandName = null;
            }

            foreach (var st in subTotals)
            {
                st.Formulas = formulas.Where(f => f.SubTotalId == st.Id)
                                      .OrderBy(f => f.DisplayOrder)
                                      .Cast<ScheduleIIISubTotalFormulaDto>()
                                      .ToList();
            }

            return subTotals;
        }

        public async Task<ScheduleIIISubTotalDto?> GetSubTotalByIdAsync(int id)
        {
            var all = await GetSubTotalsAsync();
            return all.FirstOrDefault(s => s.Id == id);
        }

        public async Task<ScheduleIIISectionItemDto?> GetLineItemByIdAsync(int id)
        {
            const string sql = @"
                SELECT li.Id, li.SectionId, li.LineCode, li.LineName, li.NoteReference,
                       li.IsSplitLine, li.IsActive
                FROM [Finance].[ScheduleIIISectionItem] li
                WHERE li.Id = @Id AND li.IsDeleted = 0;";

            return await _dbConnection.QueryFirstOrDefaultAsync<ScheduleIIISectionItemDto>(sql, new { Id = id });
        }

        public async Task<bool> LineItemNotFoundAsync(int id)
        {
            const string sql = @"SELECT COUNT(1) FROM [Finance].[ScheduleIIISectionItem] WHERE Id = @Id AND IsDeleted = 0;";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<bool> StructureExistsByCompanyDivisionAsync(int companyId, int divisionId)
        {
            const string sql = @"
                SELECT COUNT(1) FROM [Finance].[ScheduleIIIHeader]
                WHERE CompanyId = @CompanyId AND DivisionId = @DivisionId AND IsDeleted = 0;";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { CompanyId = companyId, DivisionId = divisionId });
            return count > 0;
        }

        public async Task<(List<ScheduleIIISectionDto>, int)> GetAllSectionAsync(int pageNumber, int pageSize, string? searchTerm, int? scheduleIIIMasterId)
        {
            // Section is a global catalog — scheduleIIIMasterId is ignored (kept for signature compatibility).
            var searchClause = string.IsNullOrWhiteSpace(searchTerm) ? "" : "AND sec.SectionName LIKE @Search";

            var query = $$"""
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM [Finance].[ScheduleIIISection] sec
                WHERE sec.IsDeleted = 0 {{searchClause}};

                SELECT sec.Id, sec.SectionName,
                       sec.StatementTypeId, stt.Description AS StatementTypeName,
                       sec.NatureId, nat.Description AS NatureName
                FROM [Finance].[ScheduleIIISection] sec
                LEFT JOIN [Finance].[MiscMaster] stt ON sec.StatementTypeId = stt.Id AND stt.IsDeleted = 0
                LEFT JOIN [Finance].[MiscMaster] nat ON sec.NatureId       = nat.Id AND nat.IsDeleted = 0
                WHERE sec.IsDeleted = 0 {{searchClause}}
                ORDER BY sec.SectionName
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;
            """;

            var parameters = new
            {
                Search = $"%{searchTerm}%",
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            var result = await _dbConnection.QueryMultipleAsync(query, parameters);
            var list = (await result.ReadAsync<ScheduleIIISectionDto>()).ToList();
            var totalCount = await result.ReadFirstAsync<int>();
            return (list, totalCount);
        }

        public async Task<ScheduleIIISectionDto?> GetSectionByIdAsync(int id)
        {
            const string sql = @"
                SELECT sec.Id, sec.SectionName,
                       sec.StatementTypeId, stt.Description AS StatementTypeName,
                       sec.NatureId, nat.Description AS NatureName
                FROM [Finance].[ScheduleIIISection] sec
                LEFT JOIN [Finance].[MiscMaster] stt ON sec.StatementTypeId = stt.Id AND stt.IsDeleted = 0
                LEFT JOIN [Finance].[MiscMaster] nat ON sec.NatureId       = nat.Id AND nat.IsDeleted = 0
                WHERE sec.Id = @Id AND sec.IsDeleted = 0;";

            return await _dbConnection.QueryFirstOrDefaultAsync<ScheduleIIISectionDto>(sql, new { Id = id });
        }

        public async Task<bool> SectionNotFoundAsync(int id)
        {
            const string sql = @"SELECT COUNT(1) FROM [Finance].[ScheduleIIISection] WHERE Id = @Id AND IsDeleted = 0;";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<bool> SectionNameExistsAsync(string sectionName, int? id = null)
        {
            const string sql = @"
                SELECT COUNT(1) FROM [Finance].[ScheduleIIISection]
                WHERE SectionName = @Name AND IsDeleted = 0 AND (@Id IS NULL OR Id <> @Id);";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Name = sectionName, Id = id });
            return count > 0;
        }


        public async Task<(List<ScheduleIIISectionItemDto>, int)> GetAllLineItemAsync(int pageNumber, int pageSize, string? searchTerm, int? scheduleIIIMasterId, int? sectionId)
        {
            // LineItem is a global catalog — scheduleIIIMasterId is ignored (kept for signature compatibility).
            var searchClause = string.IsNullOrWhiteSpace(searchTerm) ? "" : "AND (li.LineCode LIKE @Search OR li.LineName LIKE @Search)";
            var sectionClause = sectionId.HasValue ? "AND li.SectionId = @SectionId" : "";

            var query = $$"""
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM [Finance].[ScheduleIIISectionItem] li
                WHERE li.IsDeleted = 0 {{sectionClause}} {{searchClause}};

                SELECT li.Id, li.SectionId, sec.SectionName,
                       li.LineCode, li.LineName, li.NoteReference, li.IsSplitLine, li.IsActive
                FROM [Finance].[ScheduleIIISectionItem] li
                LEFT JOIN [Finance].[ScheduleIIISection] sec ON li.SectionId = sec.Id AND sec.IsDeleted = 0
                WHERE li.IsDeleted = 0 {{sectionClause}} {{searchClause}}
                ORDER BY li.SectionId, li.LineName
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;
            """;

            var parameters = new
            {
                Search = $"%{searchTerm}%",
                SectionId = sectionId,
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            var result = await _dbConnection.QueryMultipleAsync(query, parameters);
            var list = (await result.ReadAsync<ScheduleIIISectionItemDto>()).ToList();
            var totalCount = await result.ReadFirstAsync<int>();
            return (list, totalCount);
        }

        public async Task<bool> SectionExistsAsync(int sectionId)
        {
            const string sql = @"
                SELECT COUNT(1) FROM [Finance].[ScheduleIIISection]
                WHERE Id = @SectionId AND IsDeleted = 0;";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { SectionId = sectionId });
            return count > 0;
        }

        public async Task<bool> IsStructureLockedAsync(int companyId, int divisionId)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM [Finance].[ScheduleIIIHeader] h
                    JOIN [Finance].[MiscMaster] st ON h.StatusId = st.Id
                    WHERE h.CompanyId = @CompanyId AND h.DivisionId = @DivisionId AND h.IsDeleted = 0 AND st.Code = 'LOCKED'
                ) THEN 1 ELSE 0 END;";
            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { CompanyId = companyId, DivisionId = divisionId });
        }

        public async Task<bool> SubTotalNotFoundAsync(int id)
        {
            const string sql = @"SELECT COUNT(1) FROM [Finance].[ScheduleIIISubTotal] WHERE Id = @Id AND IsDeleted = 0;";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<bool> SubTotalNameExistsAsync(string formulaName, int? id = null)
        {
            const string sql = @"
                SELECT COUNT(1) FROM [Finance].[ScheduleIIISubTotal]
                WHERE FormulaName = @Name AND IsDeleted = 0 AND (@Id IS NULL OR Id <> @Id);";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Name = formulaName, Id = id });
            return count > 0;
        }

        public async Task<bool> SubTotalDisplayOrderExistsAsync(int displayOrder, int? id = null)
        {
            const string sql = @"
                SELECT COUNT(1) FROM [Finance].[ScheduleIIISubTotal]
                WHERE DisplayOrder = @Order AND IsDeleted = 0 AND (@Id IS NULL OR Id <> @Id);";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Order = displayOrder, Id = id });
            return count > 0;
        }

        public async Task<int> GetSubTotalOperandTypeIdAsync()
        {
            const string sql = @"
                SELECT m.Id
                FROM [Finance].[MiscMaster] m
                JOIN [Finance].[MiscTypeMaster] t ON m.MiscTypeId = t.Id
                WHERE t.MiscTypeCode = 'S3_OPERAND_TYPE' AND m.Code = 'SUBTOTAL' AND m.IsDeleted = 0;";
            return await _dbConnection.ExecuteScalarAsync<int>(sql);
        }

        // 03B usage — stubbed until US-GL02-03B ships ScheduleIIIAccountGroupMap.
        public Task<int> GetMappedCountAsync(int lineItemId) => Task.FromResult(0);

        public Task<bool> IsLineMappedAsync(int lineItemId) => Task.FromResult(false);

        private sealed class Preview03BRow : Preview03BItemDto
        {
            public string? StatementTypeCode { get; set; }
        }

        private sealed class FormulaRow : ScheduleIIISubTotalFormulaDto
        {
            public string? OperandTypeCode { get; set; }
        }

    }
}
