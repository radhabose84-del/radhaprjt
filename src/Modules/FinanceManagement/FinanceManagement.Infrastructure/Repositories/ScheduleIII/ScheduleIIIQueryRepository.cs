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

        public async Task<ScheduleIIIStructureDto?> GetStructureAsync(int companyId, int divisionId)
        {
            const string structureSql = @"
                SELECT s.Id, s.CompanyId, s.DivisionId,
                       s.StructureStatusId, st.Description AS StructureStatusName,
                       s.TextileSplitEnabled, s.VersionNo, s.IsActive
                FROM [Finance].[ScheduleIIIStructure] s
                LEFT JOIN [Finance].[MiscMaster] st ON s.StructureStatusId = st.Id AND st.IsDeleted = 0
                WHERE s.CompanyId = @CompanyId AND s.DivisionId = @DivisionId AND s.IsDeleted = 0;";

            var structure = await _dbConnection.QueryFirstOrDefaultAsync<ScheduleIIIStructureDto>(
                structureSql, new { CompanyId = companyId, DivisionId = divisionId });

            if (structure == null)
                return null;

            // Cross-module names (no DB JOIN to other modules)
            var companies = await _companyLookup.GetAllCompanyAsync();
            structure.CompanyName = companies.FirstOrDefault(c => c.CompanyId == structure.CompanyId)?.CompanyName;

            var divisions = await _divisionLookup.GetByIdsAsync(new[] { structure.DivisionId });
            structure.DivisionName = divisions.FirstOrDefault(d => d.Id == structure.DivisionId)?.Name;

            const string sectionSql = @"
                SELECT sec.Id, sec.StructureId, sec.SectionName,
                       sec.StatementTypeId, stt.Description AS StatementTypeName,
                       sec.NatureId, nat.Description AS NatureName,
                       sec.DisplayOrder
                FROM [Finance].[ScheduleIIISection] sec
                LEFT JOIN [Finance].[MiscMaster] stt ON sec.StatementTypeId = stt.Id AND stt.IsDeleted = 0
                LEFT JOIN [Finance].[MiscMaster] nat ON sec.NatureId       = nat.Id AND nat.IsDeleted = 0
                WHERE sec.StructureId = @StructureId AND sec.IsDeleted = 0
                ORDER BY sec.DisplayOrder;";

            var sections = (await _dbConnection.QueryAsync<ScheduleIIISectionDto>(
                sectionSql, new { StructureId = structure.Id })).ToList();

            const string lineSql = @"
                SELECT li.Id, li.StructureId, li.SectionId, li.ParentLineId,
                       li.LineCode, li.LineName, li.SubClassification, li.NoteReference,
                       li.DisplayOrder, li.IsSplitLine, li.IsActive
                FROM [Finance].[ScheduleIIILineItem] li
                WHERE li.StructureId = @StructureId AND li.IsDeleted = 0
                ORDER BY li.SectionId, li.DisplayOrder;";

            var lines = (await _dbConnection.QueryAsync<ScheduleIIILineItemDto>(
                lineSql, new { StructureId = structure.Id })).ToList();

            // MappedCount stays 0 until US-GL02-03B ships ScheduleIIIAccountGroupMap.
            foreach (var sec in sections)
            {
                var sectionLines = lines.Where(l => l.SectionId == sec.Id).ToList();
                var topLines = sectionLines.Where(l => l.ParentLineId == null)
                                           .OrderBy(l => l.DisplayOrder).ToList();
                foreach (var top in topLines)
                {
                    top.ChildLines = sectionLines.Where(l => l.ParentLineId == top.Id)
                                                 .OrderBy(l => l.DisplayOrder).ToList();
                }
                sec.LineItems = topLines;
            }

            structure.Sections = sections;
            structure.SubTotals = await GetSubTotalsAsync(structure.Id);
            return structure;
        }

        public async Task<ScheduleIIIStructureDto?> GetByIdAsync(int structureId)
        {
            var ids = await _dbConnection.QueryFirstOrDefaultAsync(
                "SELECT CompanyId, DivisionId FROM [Finance].[ScheduleIIIStructure] WHERE Id = @Id AND IsDeleted = 0;",
                new { Id = structureId });

            if (ids == null)
                return null;

            return await GetStructureAsync((int)ids.CompanyId, (int)ids.DivisionId);
        }

        public async Task<bool> StructureNotFoundAsync(int id)
        {
            const string sql = @"SELECT COUNT(1) FROM [Finance].[ScheduleIIIStructure] WHERE Id = @Id AND IsDeleted = 0;";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<Preview03BDto> Get03BPreviewAsync(int structureId)
        {
            const string sql = @"
                SELECT li.Id AS LineItemId, li.LineName, sec.SectionName, li.NoteReference,
                       stt.Code AS StatementTypeCode
                FROM [Finance].[ScheduleIIILineItem] li
                JOIN [Finance].[ScheduleIIISection] sec ON li.SectionId = sec.Id AND sec.IsDeleted = 0
                JOIN [Finance].[MiscMaster] stt ON sec.StatementTypeId = stt.Id AND stt.IsDeleted = 0
                WHERE li.StructureId = @StructureId AND li.IsDeleted = 0 AND li.IsActive = 1
                ORDER BY sec.DisplayOrder, li.DisplayOrder;";

            var rows = (await _dbConnection.QueryAsync<Preview03BRow>(
                sql, new { StructureId = structureId })).ToList();

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

        public async Task<List<ScheduleIIISubTotalDto>> GetSubTotalsAsync(int structureId)
        {
            const string subTotalSql = @"
                SELECT st.Id, st.StructureId, st.SubTotalName, st.FormulaExpression,
                       st.IncludeOtherIncome, st.IsSystemDefined, st.DisplayOrder, st.IsActive
                FROM [Finance].[ScheduleIIISubTotal] st
                WHERE st.StructureId = @StructureId AND st.IsDeleted = 0
                ORDER BY st.DisplayOrder;";

            var subTotals = (await _dbConnection.QueryAsync<ScheduleIIISubTotalDto>(
                subTotalSql, new { StructureId = structureId })).ToList();

            if (subTotals.Count == 0)
                return subTotals;

            const string formulaSql = @"
                SELECT f.Id, f.SubTotalId,
                       f.OperandTypeId, ot.Description AS OperandTypeName, ot.Code AS OperandTypeCode,
                       f.OperandRefId,
                       f.OperatorId, op.Description AS OperatorName,
                       f.DisplayOrder
                FROM [Finance].[ScheduleIIISubTotalFormula] f
                LEFT JOIN [Finance].[MiscMaster] ot ON f.OperandTypeId = ot.Id AND ot.IsDeleted = 0
                LEFT JOIN [Finance].[MiscMaster] op ON f.OperatorId    = op.Id AND op.IsDeleted = 0
                WHERE f.SubTotalId IN @SubTotalIds AND f.IsDeleted = 0
                ORDER BY f.DisplayOrder;";

            var formulas = (await _dbConnection.QueryAsync<FormulaRow>(
                formulaSql, new { SubTotalIds = subTotals.Select(s => s.Id).ToArray() })).ToList();

            // Resolve operand display names (line item names + sub-total names) for this structure.
            var lineNames = (await _dbConnection.QueryAsync<(int Id, string? LineName)>(
                "SELECT Id, LineName FROM [Finance].[ScheduleIIILineItem] WHERE StructureId = @StructureId AND IsDeleted = 0",
                new { StructureId = structureId })).ToDictionary(x => x.Id, x => x.LineName);

            var subTotalNames = subTotals.ToDictionary(s => s.Id, s => s.SubTotalName);

            foreach (var f in formulas)
            {
                if (string.Equals(f.OperandTypeCode, "SUBTOTAL", StringComparison.OrdinalIgnoreCase))
                    f.OperandName = subTotalNames.TryGetValue(f.OperandRefId, out var sn) ? sn : null;
                else
                    f.OperandName = lineNames.TryGetValue(f.OperandRefId, out var ln) ? ln : null;
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

        public async Task<ScheduleIIILineItemDto?> GetLineItemByIdAsync(int id)
        {
            const string sql = @"
                SELECT li.Id, li.StructureId, li.SectionId, li.ParentLineId,
                       li.LineCode, li.LineName, li.SubClassification, li.NoteReference,
                       li.DisplayOrder, li.IsSplitLine, li.IsActive
                FROM [Finance].[ScheduleIIILineItem] li
                WHERE li.Id = @Id AND li.IsDeleted = 0;";

            return await _dbConnection.QueryFirstOrDefaultAsync<ScheduleIIILineItemDto>(sql, new { Id = id });
        }

        public async Task<bool> LineItemNotFoundAsync(int id)
        {
            const string sql = @"SELECT COUNT(1) FROM [Finance].[ScheduleIIILineItem] WHERE Id = @Id AND IsDeleted = 0;";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<bool> StructureExistsAsync(int structureId)
        {
            const string sql = @"SELECT COUNT(1) FROM [Finance].[ScheduleIIIStructure] WHERE Id = @Id AND IsDeleted = 0;";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = structureId });
            return count > 0;
        }

        public async Task<bool> StructureExistsByCompanyDivisionAsync(int companyId, int divisionId)
        {
            const string sql = @"
                SELECT COUNT(1) FROM [Finance].[ScheduleIIIStructure]
                WHERE CompanyId = @CompanyId AND DivisionId = @DivisionId AND IsDeleted = 0;";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { CompanyId = companyId, DivisionId = divisionId });
            return count > 0;
        }

        public async Task<bool> SectionExistsAsync(int sectionId, int structureId)
        {
            const string sql = @"
                SELECT COUNT(1) FROM [Finance].[ScheduleIIISection]
                WHERE Id = @SectionId AND StructureId = @StructureId AND IsDeleted = 0;";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { SectionId = sectionId, StructureId = structureId });
            return count > 0;
        }

        public async Task<bool> ParentLineExistsAsync(int parentLineId, int structureId)
        {
            const string sql = @"
                SELECT COUNT(1) FROM [Finance].[ScheduleIIILineItem]
                WHERE Id = @ParentLineId AND StructureId = @StructureId AND IsDeleted = 0;";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { ParentLineId = parentLineId, StructureId = structureId });
            return count > 0;
        }

        public async Task<bool> IsStructureLockedAsync(int structureId)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM [Finance].[ScheduleIIIStructure] s
                    JOIN [Finance].[MiscMaster] st ON s.StructureStatusId = st.Id
                    WHERE s.Id = @StructureId AND s.IsDeleted = 0 AND st.Code = 'LOCKED'
                ) THEN 1 ELSE 0 END;";
            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { StructureId = structureId });
        }

        public async Task<bool> SubTotalNotFoundAsync(int id)
        {
            const string sql = @"SELECT COUNT(1) FROM [Finance].[ScheduleIIISubTotal] WHERE Id = @Id AND IsDeleted = 0;";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
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
