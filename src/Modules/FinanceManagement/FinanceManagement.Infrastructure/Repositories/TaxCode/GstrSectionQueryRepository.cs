using System.Data;
using Dapper;
using FinanceManagement.Application.Common.Interfaces.IGstrSection;
using FinanceManagement.Application.TaxCode.Dto;

namespace FinanceManagement.Infrastructure.Repositories.TaxCode
{
    public class GstrSectionQueryRepository : IGstrSectionQueryRepository
    {
        private readonly IDbConnection _dbConnection;

        public GstrSectionQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        // ─── Section Master ────────────────────────────────────────────────
        private const string SectionColumns = @"
            sm.Id, sm.CompanyId, sm.ReportTypeId, rt.Code AS ReportType,
            sm.SectionCode, sm.SectionName, sm.IsActive,
            sm.CreatedBy, sm.CreatedDate, sm.CreatedByName, sm.CreatedIP,
            sm.ModifiedBy, sm.ModifiedDate, sm.ModifiedByName, sm.ModifiedIP";

        private const string SectionFromJoins = @"
            FROM Finance.GstrSectionMaster sm
            LEFT JOIN Finance.MiscMaster rt ON sm.ReportTypeId = rt.Id";

        public async Task<(List<GstrSectionMasterDto>, int)> GetAllSectionsAsync(int pageNumber, int pageSize, string? searchTerm, int? companyId, int? reportTypeId)
        {
            var whereClause = "1 = 1";
            if (!string.IsNullOrWhiteSpace(searchTerm))
                whereClause += " AND (sm.SectionCode LIKE @Search OR sm.SectionName LIKE @Search)";
            if (companyId.HasValue && companyId.Value > 0)
                whereClause += " AND sm.CompanyId = @CompanyId";
            if (reportTypeId.HasValue && reportTypeId.Value > 0)
                whereClause += " AND sm.ReportTypeId = @ReportTypeId";

            var query = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*) FROM Finance.GstrSectionMaster sm WHERE {whereClause};

                SELECT {SectionColumns}
                {SectionFromJoins}
                WHERE {whereClause}
                ORDER BY rt.Code ASC, sm.SectionCode ASC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;";

            var parameters = new { Search = $"%{searchTerm}%", CompanyId = companyId, ReportTypeId = reportTypeId, Offset = (pageNumber - 1) * pageSize, PageSize = pageSize };
            var result = await _dbConnection.QueryMultipleAsync(query, parameters);
            var list = (await result.ReadAsync<GstrSectionMasterDto>()).ToList();
            var totalCount = await result.ReadFirstAsync<int>();
            return (list, totalCount);
        }

        public async Task<GstrSectionMasterDto?> GetSectionByIdAsync(int id)
        {
            var sql = $@"SELECT {SectionColumns} {SectionFromJoins} WHERE sm.Id = @Id";
            return await _dbConnection.QueryFirstOrDefaultAsync<GstrSectionMasterDto>(sql, new { Id = id });
        }

        public async Task<IReadOnlyList<GstrSectionMasterLookupDto>> SectionAutocompleteAsync(string term, int? reportTypeId, int? companyId, CancellationToken ct)
        {
            var whereClause = "sm.IsActive = 1";
            if (!string.IsNullOrWhiteSpace(term))
                whereClause += " AND (sm.SectionCode LIKE @Term OR sm.SectionName LIKE @Term)";
            if (companyId.HasValue && companyId.Value > 0)
                whereClause += " AND sm.CompanyId = @CompanyId";
            if (reportTypeId.HasValue && reportTypeId.Value > 0)
                whereClause += " AND sm.ReportTypeId = @ReportTypeId";

            var sql = $@"
                SELECT sm.Id, sm.ReportTypeId, rt.Code AS ReportType, sm.SectionCode, sm.SectionName
                {SectionFromJoins}
                WHERE {whereClause}
                ORDER BY rt.Code ASC, sm.SectionCode ASC";

            var result = await _dbConnection.QueryAsync<GstrSectionMasterLookupDto>(
                new CommandDefinition(sql, new { Term = $"%{term}%", ReportTypeId = reportTypeId, CompanyId = companyId }, cancellationToken: ct));
            return result.ToList();
        }

        public async Task<bool> SectionAlreadyExistsAsync(int companyId, int reportTypeId, string sectionCode, int? id = null)
        {
            var sql = @"SELECT COUNT(1) FROM Finance.GstrSectionMaster
                        WHERE CompanyId = @CompanyId AND ReportTypeId = @ReportTypeId AND SectionCode = @SectionCode";
            if (id.HasValue && id.Value > 0)
                sql += " AND Id != @Id";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { CompanyId = companyId, ReportTypeId = reportTypeId, SectionCode = sectionCode.Trim(), Id = id });
            return count > 0;
        }

        public async Task<bool> SectionNotFoundAsync(int id)
        {
            const string sql = "SELECT COUNT(1) FROM Finance.GstrSectionMaster WHERE Id = @Id";
            return await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id }) == 0;
        }

        public async Task<bool> SectionExistsAsync(int id)
        {
            const string sql = "SELECT COUNT(1) FROM Finance.GstrSectionMaster WHERE Id = @Id";
            return await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id }) > 0;
        }

        public async Task<bool> SectionHasLinkagesAsync(int id)
        {
            const string sql = @"SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Finance.GstrSectionAccountLinkage WHERE SectionMasterId = @Id
                ) THEN 1 ELSE 0 END";
            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = id });
        }

        // Valid only when the MiscMaster row is active and belongs to the GSTR_REPORT misc-type
        // (separator/case-insensitive: 'GSTR_REPORT' / 'GstrReport').
        public async Task<bool> ReportTypeExistsAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Finance.MiscMaster mm
                JOIN Finance.MiscTypeMaster mt ON mm.MiscTypeId = mt.Id AND mt.IsDeleted = 0
                WHERE mm.Id = @Id AND mm.IsActive = 1 AND mm.IsDeleted = 0
                    AND UPPER(REPLACE(REPLACE(mt.MiscTypeCode, ' ', ''), '_', '')) = 'GSTRREPORT'";
            return await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id }) > 0;
        }

        // ─── Section Account Linkage ───────────────────────────────────────
        private const string LinkageColumns = @"
            l.Id, sm.CompanyId,
            l.SectionMasterId, sm.ReportTypeId, rt.Code AS ReportType, sm.SectionCode, sm.SectionName,
            l.AccountRangeFrom, l.AccountRangeTo,
            l.DerivedValue, l.ExpectedValue, l.TolerancePercent,
            l.IsActive,
            l.CreatedBy, l.CreatedDate, l.CreatedByName,
            l.ModifiedBy, l.ModifiedDate, l.ModifiedByName";

        private const string LinkageFromJoins = @"
            FROM Finance.GstrSectionAccountLinkage l
            LEFT JOIN Finance.GstrSectionMaster sm ON l.SectionMasterId = sm.Id
            LEFT JOIN Finance.MiscMaster rt ON sm.ReportTypeId = rt.Id";

        public async Task<(List<GstrSectionAccountLinkageDto>, int)> GetAllLinkagesAsync(int pageNumber, int pageSize, string? searchTerm, int? companyId)
        {
            var whereClause = "1 = 1";
            if (!string.IsNullOrWhiteSpace(searchTerm))
                whereClause += " AND (sm.SectionCode LIKE @Search OR sm.SectionName LIKE @Search OR l.AccountRangeFrom LIKE @Search OR l.AccountRangeTo LIKE @Search)";
            if (companyId.HasValue && companyId.Value > 0)
                whereClause += " AND sm.CompanyId = @CompanyId";

            var query = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*) {LinkageFromJoins} WHERE {whereClause};

                SELECT {LinkageColumns}
                {LinkageFromJoins}
                WHERE {whereClause}
                ORDER BY rt.Code ASC, sm.SectionCode ASC, l.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;";

            var parameters = new { Search = $"%{searchTerm}%", CompanyId = companyId, Offset = (pageNumber - 1) * pageSize, PageSize = pageSize };
            var result = await _dbConnection.QueryMultipleAsync(query, parameters);
            var list = (await result.ReadAsync<GstrSectionAccountLinkageDto>()).ToList();
            var totalCount = await result.ReadFirstAsync<int>();

            foreach (var row in list)
                ApplyTolerance(row);

            return (list, totalCount);
        }

        public async Task<GstrSectionAccountLinkageDto?> GetLinkageByIdAsync(int id)
        {
            var sql = $@"SELECT {LinkageColumns} {LinkageFromJoins} WHERE l.Id = @Id";
            var dto = await _dbConnection.QueryFirstOrDefaultAsync<GstrSectionAccountLinkageDto>(sql, new { Id = id });
            if (dto != null)
                ApplyTolerance(dto);
            return dto;
        }

        public async Task<bool> LinkageNotFoundAsync(int id)
        {
            const string sql = "SELECT COUNT(1) FROM Finance.GstrSectionAccountLinkage WHERE Id = @Id";
            return await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id }) == 0;
        }

        // Derived vs Expected within TolerancePercent → "Within ±x%" / "Out of tolerance".
        private static void ApplyTolerance(GstrSectionAccountLinkageDto row)
        {
            var derived = row.DerivedValue ?? 0m;
            if (row.ExpectedValue == 0m)
            {
                row.WithinTolerance = derived == 0m;
            }
            else
            {
                var deviationPct = Math.Abs(derived - row.ExpectedValue) / Math.Abs(row.ExpectedValue) * 100m;
                row.WithinTolerance = deviationPct <= row.TolerancePercent;
            }

            row.ToleranceStatus = row.WithinTolerance
                ? $"Within ±{row.TolerancePercent:0.##}%"
                : "Out of tolerance";
        }
    }
}
