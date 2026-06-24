using System.Data;
using Contracts.Interfaces.Lookups.Users;
using Dapper;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IAccountingPeriod;
using FinanceManagement.Application.JournalMaster.Dto;

namespace FinanceManagement.Infrastructure.Repositories.JournalMaster.AccountingPeriod
{
    public class AccountingPeriodQueryRepository : IAccountingPeriodQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly ICompanyLookup _companyLookup;
        private readonly IFinancialYearLookup _financialYearLookup;

        public AccountingPeriodQueryRepository(
            IDbConnection dbConnection,
            ICompanyLookup companyLookup,
            IFinancialYearLookup financialYearLookup)
        {
            _dbConnection = dbConnection;
            _companyLookup = companyLookup;
            _financialYearLookup = financialYearLookup;
        }

        public async Task<(List<AccountingPeriodDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm, int? companyId = null, int? financialYearId = null)
        {
            var whereClause = "ap.IsDeleted = 0";
            if (!string.IsNullOrWhiteSpace(searchTerm))
                whereClause += " AND ap.PeriodName LIKE @Search";
            if (companyId.HasValue && companyId.Value > 0)
                whereClause += " AND ap.CompanyId = @CompanyId";
            if (financialYearId.HasValue && financialYearId.Value > 0)
                whereClause += " AND ap.FinancialYearId = @FinancialYearId";

            var query = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Finance.AccountingPeriod ap
                WHERE {whereClause};

                SELECT ap.Id, ap.CompanyId, ap.FinancialYearId, ap.PeriodName, ap.PeriodNo,
                    ap.StartDate, ap.EndDate, ap.StatusId, mm.Description AS StatusName,
                    ap.IsActive, ap.IsDeleted,
                    ap.CreatedBy, ap.CreatedDate, ap.CreatedByName, ap.CreatedIP,
                    ap.ModifiedBy, ap.ModifiedDate, ap.ModifiedByName, ap.ModifiedIP
                FROM Finance.AccountingPeriod ap
                LEFT JOIN Finance.MiscMaster mm ON mm.Id = ap.StatusId AND mm.IsDeleted = 0
                WHERE {whereClause}
                ORDER BY ap.FinancialYearId ASC, ap.PeriodNo ASC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;
            ";

            var parameters = new
            {
                Search = $"%{searchTerm}%",
                CompanyId = companyId,
                FinancialYearId = financialYearId,
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            var multi = await _dbConnection.QueryMultipleAsync(query, parameters);
            var list = (await multi.ReadAsync<AccountingPeriodDto>()).ToList();
            var totalCount = await multi.ReadFirstAsync<int>();

            if (list.Count > 0)
            {
                var companies = await _companyLookup.GetAllCompanyAsync();
                var companyDict = companies.ToDictionary(c => c.CompanyId, c => c.CompanyName);

                var years = await _financialYearLookup.GetAllFinancialYearAsync();
                var yearDict = years.ToDictionary(y => y.FinancialYearId, y => y.FinancialYearName);

                foreach (var item in list)
                {
                    item.CompanyName = companyDict.TryGetValue(item.CompanyId, out var cName) ? cName : null;
                    item.FinancialYearName = yearDict.TryGetValue(item.FinancialYearId, out var yName) ? yName : null;
                }
            }

            return (list, totalCount);
        }

        public async Task<AccountingPeriodDto?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT ap.Id, ap.CompanyId, ap.FinancialYearId, ap.PeriodName, ap.PeriodNo,
                    ap.StartDate, ap.EndDate, ap.StatusId, mm.Description AS StatusName,
                    ap.IsActive, ap.IsDeleted,
                    ap.CreatedBy, ap.CreatedDate, ap.CreatedByName, ap.CreatedIP,
                    ap.ModifiedBy, ap.ModifiedDate, ap.ModifiedByName, ap.ModifiedIP
                FROM Finance.AccountingPeriod ap
                LEFT JOIN Finance.MiscMaster mm ON mm.Id = ap.StatusId AND mm.IsDeleted = 0
                WHERE ap.Id = @Id AND ap.IsDeleted = 0";

            var dto = await _dbConnection.QueryFirstOrDefaultAsync<AccountingPeriodDto>(sql, new { Id = id });

            if (dto != null)
            {
                var companies = await _companyLookup.GetAllCompanyAsync();
                dto.CompanyName = companies.FirstOrDefault(c => c.CompanyId == dto.CompanyId)?.CompanyName;

                var years = await _financialYearLookup.GetAllFinancialYearAsync();
                dto.FinancialYearName = years.FirstOrDefault(y => y.FinancialYearId == dto.FinancialYearId)?.FinancialYearName;
            }

            return dto;
        }

        public async Task<IReadOnlyList<AccountingPeriodLookupDto>> AutocompleteAsync(string term, int? companyId, int? financialYearId, CancellationToken ct)
        {
            var whereClause = "ap.IsDeleted = 0 AND ap.IsActive = 1";
            if (!string.IsNullOrWhiteSpace(term))
                whereClause += " AND ap.PeriodName LIKE @Term";
            if (companyId.HasValue && companyId.Value > 0)
                whereClause += " AND ap.CompanyId = @CompanyId";
            if (financialYearId.HasValue && financialYearId.Value > 0)
                whereClause += " AND ap.FinancialYearId = @FinancialYearId";

            var sql = $@"
                SELECT ap.Id, ap.PeriodNo, ap.PeriodName
                FROM Finance.AccountingPeriod ap
                WHERE {whereClause}
                ORDER BY ap.FinancialYearId ASC, ap.PeriodNo ASC";

            var result = await _dbConnection.QueryAsync<AccountingPeriodLookupDto>(
                new CommandDefinition(sql, new { Term = $"%{term}%", CompanyId = companyId, FinancialYearId = financialYearId }, cancellationToken: ct));
            return result.ToList();
        }

        public async Task<bool> AlreadyExistsAsync(int companyId, int financialYearId, int periodNo, int? id = null)
        {
            var sql = @"
                SELECT COUNT(1)
                FROM Finance.AccountingPeriod
                WHERE CompanyId = @CompanyId AND FinancialYearId = @FinancialYearId
                    AND PeriodNo = @PeriodNo AND IsDeleted = 0";

            if (id.HasValue && id.Value > 0)
                sql += " AND Id != @Id";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql,
                new { CompanyId = companyId, FinancialYearId = financialYearId, PeriodNo = periodNo, Id = id });
            return count > 0;
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Finance.AccountingPeriod
                WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<bool> StatusExistsAsync(int statusId)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1
                    FROM Finance.MiscMaster mm
                    INNER JOIN Finance.MiscTypeMaster mt ON mt.Id = mm.MiscTypeId AND mt.IsDeleted = 0
                    WHERE mm.Id = @StatusId AND mm.IsActive = 1 AND mm.IsDeleted = 0
                        AND mt.MiscTypeCode = 'PERIOD_STATUS'
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { StatusId = statusId });
        }
    }
}
