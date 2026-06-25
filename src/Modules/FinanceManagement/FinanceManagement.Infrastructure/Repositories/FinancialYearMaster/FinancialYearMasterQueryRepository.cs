using System.Data;
using Contracts.Interfaces.Lookups.Users;
using Dapper;
using FinanceManagement.Application.Common.Interfaces.IFinancialYearMaster;
using FinanceManagement.Application.FinancialYearMaster.Dto;

namespace FinanceManagement.Infrastructure.Repositories.FinancialYearMaster
{
    public class FinancialYearMasterQueryRepository : IFinancialYearMasterQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly ICompanyLookup _companyLookup;

        public FinancialYearMasterQueryRepository(IDbConnection dbConnection, ICompanyLookup companyLookup)
        {
            _dbConnection = dbConnection;
            _companyLookup = companyLookup;
        }

        private const string YearSelect = @"
            fy.Id, fy.CompanyId, fy.FinancialYearCode, fy.StartDate, fy.EndDate,
            fy.StatusId, fys.Code AS StatusCode, fys.Description AS StatusName,
            fy.IsTransitionYear, fy.IsActive, fy.IsDeleted,
            fy.CreatedBy, fy.CreatedDate, fy.CreatedByName, fy.CreatedIP,
            fy.ModifiedBy, fy.ModifiedDate, fy.ModifiedByName, fy.ModifiedIP
        ";

        private const string YearFromJoins = @"
            FROM Finance.FinancialYearMaster fy
            LEFT JOIN Finance.MiscMaster fys ON fy.StatusId = fys.Id AND fys.IsDeleted = 0
        ";

        private const string PeriodSelect = @"
            fp.Id, fp.FinancialYearId, pfy.FinancialYearCode, fp.CompanyId,
            fp.PeriodNumber, fp.PeriodName, fp.StartDate, fp.EndDate,
            fp.StatusId, fps.Code AS StatusCode, fps.Description AS StatusName,
            fp.IsAdjustmentPeriod, fp.IsActive, fp.IsDeleted
        ";

        private const string PeriodFromJoins = @"
            FROM Finance.FinancialPeriodMaster fp
            LEFT JOIN Finance.FinancialYearMaster pfy ON fp.FinancialYearId = pfy.Id AND pfy.IsDeleted = 0
            LEFT JOIN Finance.MiscMaster fps ON fp.StatusId = fps.Id AND fps.IsDeleted = 0
        ";

        public async Task<(List<FinancialYearMasterDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm, int companyId, int? statusId = null)
        {
            var whereClause = "fy.IsDeleted = 0 AND fy.CompanyId = @CompanyId";
            if (!string.IsNullOrWhiteSpace(searchTerm))
                whereClause += " AND fy.FinancialYearCode LIKE @Search";
            if (statusId.HasValue && statusId.Value > 0)
                whereClause += " AND fy.StatusId = @StatusId";

            var query = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Finance.FinancialYearMaster fy
                WHERE {whereClause};

                SELECT {YearSelect}
                {YearFromJoins}
                WHERE {whereClause}
                ORDER BY fy.StartDate DESC, fy.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;
            ";

            var parameters = new
            {
                Search = $"%{searchTerm}%",
                CompanyId = companyId,
                StatusId = statusId,
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            var result = await _dbConnection.QueryMultipleAsync(query, parameters);
            var list = (await result.ReadAsync<FinancialYearMasterDto>()).ToList();
            var totalCount = await result.ReadFirstAsync<int>();

            if (list.Count > 0)
            {
                var companies = await _companyLookup.GetAllCompanyAsync();
                var companyDict = companies.ToDictionary(c => c.CompanyId, c => c.CompanyName);
                foreach (var item in list)
                    item.CompanyName = companyDict.TryGetValue(item.CompanyId, out var name) ? name : null;
            }

            return (list, totalCount);
        }

        public async Task<FinancialYearMasterDto?> GetByIdAsync(int id)
        {
            var headerSql = $@"
                SELECT {YearSelect}
                {YearFromJoins}
                WHERE fy.Id = @Id AND fy.IsDeleted = 0";

            var dto = await _dbConnection.QueryFirstOrDefaultAsync<FinancialYearMasterDto>(headerSql, new { Id = id });
            if (dto == null) return null;

            var periodsSql = $@"
                SELECT {PeriodSelect}
                {PeriodFromJoins}
                WHERE fp.FinancialYearId = @Id AND fp.IsDeleted = 0
                ORDER BY fp.PeriodNumber ASC";

            var periods = (await _dbConnection.QueryAsync<FinancialPeriodMasterDto>(periodsSql, new { Id = id })).ToList();
            dto.Periods = periods;

            var companies = await _companyLookup.GetAllCompanyAsync();
            dto.CompanyName = companies.FirstOrDefault(c => c.CompanyId == dto.CompanyId)?.CompanyName;

            return dto;
        }

        public async Task<IReadOnlyList<FinancialPeriodMasterDto>> GetPeriodsForCompanyAsync(int companyId, CancellationToken ct)
        {
            var sql = $@"
                SELECT {PeriodSelect}
                {PeriodFromJoins}
                WHERE fp.CompanyId = @CompanyId AND fp.IsDeleted = 0
                ORDER BY fp.StartDate ASC, fp.PeriodNumber ASC";

            var result = await _dbConnection.QueryAsync<FinancialPeriodMasterDto>(
                new CommandDefinition(sql, new { CompanyId = companyId }, cancellationToken: ct));
            return result.ToList();
        }

        public async Task<FinancialPeriodMasterDto?> GetPeriodForDateAsync(int companyId, DateOnly date, CancellationToken ct)
        {
            var sql = $@"
                SELECT TOP 1 {PeriodSelect}
                {PeriodFromJoins}
                WHERE fp.CompanyId = @CompanyId
                  AND fp.IsDeleted = 0
                  AND fp.IsAdjustmentPeriod = 0          -- date resolution always returns the regular period
                  AND @Date BETWEEN fp.StartDate AND fp.EndDate
                ORDER BY fp.PeriodNumber ASC";

            return await _dbConnection.QueryFirstOrDefaultAsync<FinancialPeriodMasterDto>(
                new CommandDefinition(sql, new { CompanyId = companyId, Date = date }, cancellationToken: ct));
        }

        public async Task<IReadOnlyList<FinancialYearMasterLookupDto>> AutocompleteAsync(string term, int companyId, CancellationToken ct)
        {
            var whereClause = "fy.IsDeleted = 0 AND fy.IsActive = 1 AND fy.CompanyId = @CompanyId";
            if (!string.IsNullOrWhiteSpace(term))
                whereClause += " AND fy.FinancialYearCode LIKE @Term";

            var sql = $@"
                SELECT fy.Id, fy.CompanyId, fy.FinancialYearCode, fy.StartDate, fy.EndDate,
                       fy.StatusId, fys.Code AS StatusCode
                FROM Finance.FinancialYearMaster fy
                LEFT JOIN Finance.MiscMaster fys ON fy.StatusId = fys.Id AND fys.IsDeleted = 0
                WHERE {whereClause}
                ORDER BY fy.StartDate DESC";

            var result = await _dbConnection.QueryAsync<FinancialYearMasterLookupDto>(
                new CommandDefinition(sql, new { Term = $"%{term}%", CompanyId = companyId }, cancellationToken: ct));
            return result.ToList();
        }

        public async Task<FinancialYearMasterLookupDto?> GetLatestForCompanyAsync(int companyId, CancellationToken ct)
        {
            const string sql = @"
                SELECT TOP 1 fy.Id, fy.CompanyId, fy.FinancialYearCode, fy.StartDate, fy.EndDate,
                       fy.StatusId, fys.Code AS StatusCode
                FROM Finance.FinancialYearMaster fy
                LEFT JOIN Finance.MiscMaster fys ON fy.StatusId = fys.Id AND fys.IsDeleted = 0
                WHERE fy.CompanyId = @CompanyId AND fy.IsDeleted = 0
                ORDER BY fy.EndDate DESC";

            return await _dbConnection.QueryFirstOrDefaultAsync<FinancialYearMasterLookupDto>(
                new CommandDefinition(sql, new { CompanyId = companyId }, cancellationToken: ct));
        }

        public async Task<bool> AlreadyExistsByCodeAsync(string financialYearCode, int companyId, int? id = null)
        {
            var sql = @"
                SELECT COUNT(1)
                FROM Finance.FinancialYearMaster
                WHERE FinancialYearCode = @Code AND CompanyId = @CompanyId AND IsDeleted = 0";
            if (id.HasValue && id.Value > 0)
                sql += " AND Id != @Id";

            var count = await _dbConnection.ExecuteScalarAsync<int>(
                sql, new { Code = financialYearCode.Trim(), CompanyId = companyId, Id = id });
            return count > 0;
        }

        public async Task<bool> OverlapsExistingRangeAsync(DateOnly startDate, DateOnly endDate, int companyId, int? id = null)
        {
            var sql = @"
                SELECT COUNT(1)
                FROM Finance.FinancialYearMaster
                WHERE CompanyId = @CompanyId AND IsDeleted = 0
                  AND (@StartDate <= EndDate AND @EndDate >= StartDate)";
            if (id.HasValue && id.Value > 0)
                sql += " AND Id != @Id";

            var count = await _dbConnection.ExecuteScalarAsync<int>(
                sql, new { StartDate = startDate, EndDate = endDate, CompanyId = companyId, Id = id });
            return count > 0;
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Finance.FinancialYearMaster
                WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<bool> FinancialYearStatusExistsAsync(int statusId)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Finance.MiscMaster mm
                INNER JOIN Finance.MiscTypeMaster mtm ON mm.MiscTypeId = mtm.Id AND mtm.IsDeleted = 0
                WHERE mm.Id = @Id AND mm.IsActive = 1 AND mm.IsDeleted = 0
                  AND mtm.MiscTypeCode = 'FYS'";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = statusId });
            return count > 0;
        }

        public async Task<bool> FinancialPeriodStatusExistsAsync(int statusId)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Finance.MiscMaster mm
                INNER JOIN Finance.MiscTypeMaster mtm ON mm.MiscTypeId = mtm.Id AND mtm.IsDeleted = 0
                WHERE mm.Id = @Id AND mm.IsActive = 1 AND mm.IsDeleted = 0
                  AND mtm.MiscTypeCode = 'FPS'";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = statusId });
            return count > 0;
        }

        public async Task<int> GetMiscMasterIdByCodeAsync(string miscTypeCode, string valueCode)
        {
            const string sql = @"
                SELECT TOP 1 mm.Id
                FROM Finance.MiscMaster mm
                INNER JOIN Finance.MiscTypeMaster mtm ON mm.MiscTypeId = mtm.Id AND mtm.IsDeleted = 0
                WHERE mtm.MiscTypeCode = @TypeCode
                  AND mm.Code = @ValueCode
                  AND mm.IsActive = 1 AND mm.IsDeleted = 0";

            return await _dbConnection.ExecuteScalarAsync<int>(sql, new { TypeCode = miscTypeCode, ValueCode = valueCode });
        }

        // TODO(GL-01-FR-009): replace with real check against Finance.JournalDetail when posting engine lands.
        public Task<bool> SoftDeleteValidationAsync(int id) => Task.FromResult(false);

        public Task<bool> IsFinancialYearLinkedAsync(int id) => Task.FromResult(false);
    }
}
