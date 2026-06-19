using System.Data;
using Contracts.Interfaces.Lookups.Users;
using Dapper;
using FinanceManagement.Application.Common;
using FinanceManagement.Application.Common.Interfaces.IVoucherTypeMaster;
using FinanceManagement.Application.VoucherType.Dto;

namespace FinanceManagement.Infrastructure.Repositories.VoucherType
{
    public class VoucherTypeMasterQueryRepository : IVoucherTypeMasterQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly ICompanyLookup _companyLookup;
        private readonly IFinancialYearLookup _financialYearLookup;

        public VoucherTypeMasterQueryRepository(
            IDbConnection dbConnection,
            ICompanyLookup companyLookup,
            IFinancialYearLookup financialYearLookup)
        {
            _dbConnection = dbConnection;
            _companyLookup = companyLookup;
            _financialYearLookup = financialYearLookup;
        }

        public async Task<(List<VoucherTypeMasterDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm, int? companyId = null, int? financialYearId = null)
        {
            // Resolve the fiscal year for "Next No. (this FY)": an explicit FY (the tab the UI is on)
            // wins; otherwise the current FY by date. NEVER FirstOrDefault(IsActive) — many FYs are active.
            var years = await _financialYearLookup.GetAllFinancialYearAsync();
            var activeYear = financialYearId.HasValue && financialYearId.Value > 0
                ? years.FirstOrDefault(y => y.FinancialYearId == financialYearId.Value)
                : FinancialYearResolver.ResolveCurrent(years);
            var activeYearId = activeYear?.FinancialYearId ?? 0;

            var whereClause = "vt.IsDeleted = 0";
            if (!string.IsNullOrWhiteSpace(searchTerm))
                whereClause += " AND (vt.VoucherTypeCode LIKE @Search OR vt.VoucherTypeName LIKE @Search)";
            if (companyId.HasValue && companyId.Value > 0)
                whereClause += " AND vt.CompanyId = @CompanyId";

            var query = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Finance.VoucherTypeMaster vt
                WHERE {whereClause};

                SELECT vt.Id, vt.CompanyId, vt.VoucherTypeCode, vt.VoucherTypeName, vt.NumberPadding, vt.IsSystem,
                    vt.IsActive, vt.IsDeleted,
                    vt.CreatedBy, vt.CreatedDate, vt.CreatedByName, vt.CreatedIP,
                    vt.ModifiedBy, vt.ModifiedDate, vt.ModifiedByName, vt.ModifiedIP,
                    ISNULL(ns.LastUsedNumber, 0) AS LastUsedNumber
                FROM Finance.VoucherTypeMaster vt
                LEFT JOIN Finance.VoucherTypeNumberSeries ns
                    ON ns.VoucherTypeId = vt.Id AND ns.FinancialYearId = @ActiveYearId AND ns.IsDeleted = 0
                WHERE {whereClause}
                ORDER BY vt.CompanyId ASC, vt.Id ASC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;
            ";

            var parameters = new
            {
                Search = $"%{searchTerm}%",
                CompanyId = companyId,
                ActiveYearId = activeYearId,
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            var multi = await _dbConnection.QueryMultipleAsync(query, parameters);
            var list = (await multi.ReadAsync<VoucherTypeMasterDto>()).ToList();
            var totalCount = await multi.ReadFirstAsync<int>();

            if (list.Count > 0)
            {
                await PopulateAllowedAccountTypesAsync(list);

                var companies = await _companyLookup.GetAllCompanyAsync();
                var companyDict = companies.ToDictionary(c => c.CompanyId, c => c.CompanyName);

                foreach (var item in list)
                {
                    item.CompanyName = companyDict.TryGetValue(item.CompanyId, out var name) ? name : null;
                    item.FinancialYearId = activeYearId;
                    item.FinancialYearName = activeYear?.FinancialYearName;
                    item.NextNumber = FormatNextNumber(item.VoucherTypeCode, activeYear?.FinancialYearName, item.LastUsedNumber, item.NumberPadding);
                }
            }

            return (list, totalCount);
        }

        public async Task<VoucherTypeMasterDto?> GetByIdAsync(int id)
        {
            var years = await _financialYearLookup.GetAllFinancialYearAsync();
            var activeYear = FinancialYearResolver.ResolveCurrent(years);
            var activeYearId = activeYear?.FinancialYearId ?? 0;

            const string sql = @"
                SELECT vt.Id, vt.CompanyId, vt.VoucherTypeCode, vt.VoucherTypeName, vt.NumberPadding, vt.IsSystem,
                    vt.IsActive, vt.IsDeleted,
                    vt.CreatedBy, vt.CreatedDate, vt.CreatedByName, vt.CreatedIP,
                    vt.ModifiedBy, vt.ModifiedDate, vt.ModifiedByName, vt.ModifiedIP,
                    ISNULL(ns.LastUsedNumber, 0) AS LastUsedNumber
                FROM Finance.VoucherTypeMaster vt
                LEFT JOIN Finance.VoucherTypeNumberSeries ns
                    ON ns.VoucherTypeId = vt.Id AND ns.FinancialYearId = @ActiveYearId AND ns.IsDeleted = 0
                WHERE vt.Id = @Id AND vt.IsDeleted = 0";

            var dto = await _dbConnection.QueryFirstOrDefaultAsync<VoucherTypeMasterDto>(sql, new { Id = id, ActiveYearId = activeYearId });

            if (dto != null)
            {
                await PopulateAllowedAccountTypesAsync(new List<VoucherTypeMasterDto> { dto });

                var companies = await _companyLookup.GetAllCompanyAsync();
                dto.CompanyName = companies.FirstOrDefault(c => c.CompanyId == dto.CompanyId)?.CompanyName;
                dto.FinancialYearId = activeYearId;
                dto.FinancialYearName = activeYear?.FinancialYearName;
                dto.NextNumber = FormatNextNumber(dto.VoucherTypeCode, activeYear?.FinancialYearName, dto.LastUsedNumber, dto.NumberPadding);
            }

            return dto;
        }

        public async Task<IReadOnlyList<VoucherTypeLookupDto>> AutocompleteAsync(string term, int? companyId, CancellationToken ct)
        {
            var whereClause = "vt.IsDeleted = 0 AND vt.IsActive = 1";
            if (!string.IsNullOrWhiteSpace(term))
                whereClause += " AND (vt.VoucherTypeCode LIKE @Term OR vt.VoucherTypeName LIKE @Term)";
            if (companyId.HasValue && companyId.Value > 0)
                whereClause += " AND vt.CompanyId = @CompanyId";

            var sql = $@"
                SELECT vt.Id, vt.VoucherTypeCode, vt.VoucherTypeName
                FROM Finance.VoucherTypeMaster vt
                WHERE {whereClause}
                ORDER BY vt.VoucherTypeCode ASC";

            var result = await _dbConnection.QueryAsync<VoucherTypeLookupDto>(
                new CommandDefinition(sql, new { Term = $"%{term}%", CompanyId = companyId }, cancellationToken: ct));
            return result.ToList();
        }

        public async Task<VoucherTypeSummaryDto> GetSummaryAsync(int? companyId)
        {
            var whereClause = "IsDeleted = 0";
            if (companyId.HasValue && companyId.Value > 0)
                whereClause += " AND CompanyId = @CompanyId";

            var sql = $@"
                SELECT
                    COUNT(*) AS TotalCount,
                    SUM(CASE WHEN IsActive = 1 THEN 1 ELSE 0 END) AS ActiveCount,
                    SUM(CASE WHEN IsSystem = 1 THEN 1 ELSE 0 END) AS SystemCount,
                    SUM(CASE WHEN IsSystem = 0 THEN 1 ELSE 0 END) AS CustomCount
                FROM Finance.VoucherTypeMaster
                WHERE {whereClause}";

            var summary = await _dbConnection.QueryFirstOrDefaultAsync<VoucherTypeSummaryDto>(sql, new { CompanyId = companyId });
            return summary ?? new VoucherTypeSummaryDto();
        }

        public async Task<List<VoucherTypeNumberSeriesDto>> GetNumberSeriesAsync(int financialYearId, int? companyId)
        {
            var years = await _financialYearLookup.GetByIdAsync(financialYearId);
            var yearName = years?.FinancialYearName;

            var whereClause = "vt.IsDeleted = 0";
            if (companyId.HasValue && companyId.Value > 0)
                whereClause += " AND vt.CompanyId = @CompanyId";

            var sql = $@"
                SELECT vt.Id AS VoucherTypeId, vt.VoucherTypeCode, vt.VoucherTypeName, vt.NumberPadding, vt.IsSystem,
                    ISNULL(ns.LastUsedNumber, 0) AS LastUsedNumber
                FROM Finance.VoucherTypeMaster vt
                LEFT JOIN Finance.VoucherTypeNumberSeries ns
                    ON ns.VoucherTypeId = vt.Id AND ns.FinancialYearId = @FinancialYearId AND ns.IsDeleted = 0
                WHERE {whereClause}
                ORDER BY vt.VoucherTypeCode ASC";

            var rows = (await _dbConnection.QueryAsync<VoucherTypeNumberSeriesDto>(
                sql, new { FinancialYearId = financialYearId, CompanyId = companyId })).ToList();

            foreach (var row in rows)
            {
                row.FinancialYearId = financialYearId;
                row.FinancialYearName = yearName;
                row.NextNumberValue = row.LastUsedNumber + 1;
                row.NextNumber = FormatNextNumber(row.VoucherTypeCode, yearName, row.LastUsedNumber, row.NumberPadding);
            }

            return rows;
        }

        public async Task<bool> AlreadyExistsByCodeAsync(string voucherTypeCode, int companyId, int? id = null)
        {
            var sql = @"
                SELECT COUNT(1)
                FROM Finance.VoucherTypeMaster
                WHERE VoucherTypeCode = @VoucherTypeCode AND CompanyId = @CompanyId AND IsDeleted = 0";

            if (id.HasValue && id.Value > 0)
                sql += " AND Id != @Id";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql,
                new { VoucherTypeCode = voucherTypeCode.Trim(), CompanyId = companyId, Id = id });
            return count > 0;
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Finance.VoucherTypeMaster
                WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<bool> IsSystemAsync(int id)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Finance.VoucherTypeMaster
                    WHERE Id = @Id AND IsSystem = 1 AND IsDeleted = 0
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = id });
        }

        public async Task<bool> AccountTypeExistsAsync(int accountTypeId, int companyId)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Finance.AccountTypeMaster
                    WHERE Id = @AccountTypeId AND CompanyId = @CompanyId AND IsDeleted = 0
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { AccountTypeId = accountTypeId, CompanyId = companyId });
        }

        // Delete guard (Rule #25): blocked when the type has consumed a number in any fiscal year.
        public async Task<bool> SoftDeleteValidationAsync(int id)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Finance.VoucherTypeNumberSeries
                    WHERE VoucherTypeId = @Id AND LastUsedNumber > 0 AND IsDeleted = 0
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = id });
        }

        private async Task PopulateAllowedAccountTypesAsync(List<VoucherTypeMasterDto> masters)
        {
            var ids = masters.Select(m => m.Id).ToList();

            const string sql = @"
                SELECT vat.VoucherTypeId, vat.AccountTypeId, atm.AccountTypeName
                FROM Finance.VoucherTypeAccountType vat
                INNER JOIN Finance.AccountTypeMaster atm ON atm.Id = vat.AccountTypeId AND atm.IsDeleted = 0
                WHERE vat.VoucherTypeId IN @Ids AND vat.IsDeleted = 0
                ORDER BY atm.SortOrder ASC, atm.Id ASC";

            var rows = await _dbConnection.QueryAsync<AllowedAccountTypeRow>(sql, new { Ids = ids });
            var grouped = rows
                .GroupBy(r => r.VoucherTypeId)
                .ToDictionary(g => g.Key, g => g.Select(r => new VoucherTypeAccountTypeDto
                {
                    AccountTypeId = r.AccountTypeId,
                    AccountTypeName = r.AccountTypeName
                }).ToList());

            foreach (var master in masters)
            {
                master.AllowedAccountTypes = grouped.TryGetValue(master.Id, out var types) ? types : new List<VoucherTypeAccountTypeDto>();
            }
        }

        private static string FormatNextNumber(string? code, string? financialYearName, int lastUsedNumber, int numberPadding)
        {
            var padding = numberPadding > 0 ? numberPadding : 4;
            var next = (lastUsedNumber + 1).ToString().PadLeft(padding, '0');
            return $"{code}/{financialYearName ?? "????"}/{next}";
        }

        private sealed class AllowedAccountTypeRow
        {
            public int VoucherTypeId { get; set; }
            public int AccountTypeId { get; set; }
            public string? AccountTypeName { get; set; }
        }
    }
}
