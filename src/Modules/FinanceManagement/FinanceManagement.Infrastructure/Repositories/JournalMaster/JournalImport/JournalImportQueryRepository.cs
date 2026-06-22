using System.Data;
using Dapper;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournalImport;
using FinanceManagement.Application.JournalMaster.Dto;

namespace FinanceManagement.Infrastructure.Repositories.JournalMaster.JournalImport
{
    public class JournalImportQueryRepository : IJournalImportQueryRepository
    {
        private readonly IDbConnection _dbConnection;

        public JournalImportQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<(List<JournalImportBatchDto>, int)> GetAllBatchesAsync(int pageNumber, int pageSize)
        {
            const string query = @"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*) FROM Finance.JournalImportBatch WHERE IsDeleted = 0;

                SELECT b.Id, b.FileName, b.TotalRows, b.ValidRows, b.ErrorRows, b.StatusId, mm.Description AS StatusName,
                    b.ImportedBy, b.IsActive, b.IsDeleted, b.CreatedDate, b.CreatedByName
                FROM Finance.JournalImportBatch b
                LEFT JOIN Finance.MiscMaster mm ON mm.Id = b.StatusId
                WHERE b.IsDeleted = 0
                ORDER BY b.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;";

            var multi = await _dbConnection.QueryMultipleAsync(query, new { Offset = (pageNumber - 1) * pageSize, PageSize = pageSize });
            var list = (await multi.ReadAsync<JournalImportBatchDto>()).ToList();
            var totalCount = await multi.ReadFirstAsync<int>();
            return (list, totalCount);
        }

        public async Task<JournalImportBatchDto?> GetBatchByIdAsync(int id)
        {
            const string sql = @"
                SELECT b.Id, b.FileName, b.TotalRows, b.ValidRows, b.ErrorRows, b.StatusId, mm.Description AS StatusName,
                    b.ImportedBy, b.IsActive, b.IsDeleted, b.CreatedDate, b.CreatedByName
                FROM Finance.JournalImportBatch b
                LEFT JOIN Finance.MiscMaster mm ON mm.Id = b.StatusId
                WHERE b.Id = @Id AND b.IsDeleted = 0;

                SELECT e.RowNo, e.ColumnName, e.Message
                FROM Finance.JournalImportError e
                WHERE e.ImportBatchId = @Id
                ORDER BY e.RowNo ASC, e.Id ASC;";

            var multi = await _dbConnection.QueryMultipleAsync(sql, new { Id = id });
            var dto = await multi.ReadFirstOrDefaultAsync<JournalImportBatchDto>();
            if (dto == null)
                return null;

            dto.Errors = (await multi.ReadAsync<JournalImportErrorDto>()).ToList();
            return dto;
        }

        public async Task<int> GetStatusIdAsync(string code) => await GetMiscIdAsync("JOURNAL_STATUS", code);
        public async Task<int> GetSourceIdAsync(string code) => await GetMiscIdAsync("JOURNAL_SOURCE", code);
        public async Task<int> GetBatchStatusIdAsync(string code) => await GetMiscIdAsync("IMPORT_BATCH_STATUS", code);

        private async Task<int> GetMiscIdAsync(string typeCode, string code)
        {
            const string sql = @"
                SELECT mm.Id
                FROM Finance.MiscMaster mm
                INNER JOIN Finance.MiscTypeMaster mt ON mt.Id = mm.MiscTypeId AND mt.IsDeleted = 0
                WHERE mt.MiscTypeCode = @TypeCode AND mm.Code = @Code AND mm.IsActive = 1 AND mm.IsDeleted = 0";
            return await _dbConnection.ExecuteScalarAsync<int>(sql, new { TypeCode = typeCode, Code = code });
        }

        public async Task<IReadOnlyCollection<int>> GetExistingGlAccountIdsAsync(IEnumerable<int> ids, int companyId)
        {
            var list = ids.Where(i => i > 0).Distinct().ToList();
            if (list.Count == 0) return Array.Empty<int>();
            const string sql = @"SELECT Id FROM Finance.GlAccountMaster WHERE Id IN @Ids AND CompanyId = @CompanyId AND IsActive = 1 AND IsDeleted = 0";
            return (await _dbConnection.QueryAsync<int>(sql, new { Ids = list, CompanyId = companyId })).ToList();
        }

        public async Task<IReadOnlyCollection<int>> GetExistingVoucherTypeIdsAsync(IEnumerable<int> ids, int companyId)
        {
            var list = ids.Where(i => i > 0).Distinct().ToList();
            if (list.Count == 0) return Array.Empty<int>();
            const string sql = @"SELECT Id FROM Finance.VoucherTypeMaster WHERE Id IN @Ids AND CompanyId = @CompanyId AND IsActive = 1 AND IsDeleted = 0";
            return (await _dbConnection.QueryAsync<int>(sql, new { Ids = list, CompanyId = companyId })).ToList();
        }

        public async Task<IReadOnlyCollection<int>> GetExistingCostCentreIdsAsync(IEnumerable<int> ids)
        {
            var list = ids.Where(i => i > 0).Distinct().ToList();
            if (list.Count == 0) return Array.Empty<int>();
            const string sql = @"SELECT Id FROM Finance.CostCentre WHERE Id IN @Ids AND IsActive = 1 AND IsDeleted = 0";
            return (await _dbConnection.QueryAsync<int>(sql, new { Ids = list })).ToList();
        }

        public async Task<IReadOnlyCollection<int>> GetExistingProfitCentreIdsAsync(IEnumerable<int> ids)
        {
            var list = ids.Where(i => i > 0).Distinct().ToList();
            if (list.Count == 0) return Array.Empty<int>();
            const string sql = @"SELECT Id FROM Finance.ProfitCentre WHERE Id IN @Ids AND IsActive = 1 AND IsDeleted = 0";
            return (await _dbConnection.QueryAsync<int>(sql, new { Ids = list })).ToList();
        }

        public async Task<IReadOnlyCollection<int>> GetExistingCurrencyIdsAsync(IEnumerable<int> ids)
        {
            var list = ids.Where(i => i > 0).Distinct().ToList();
            if (list.Count == 0) return Array.Empty<int>();
            const string sql = @"SELECT Id FROM Finance.CurrencyForexConfig WHERE Id IN @Ids AND IsDeleted = 0";
            return (await _dbConnection.QueryAsync<int>(sql, new { Ids = list })).ToList();
        }

        public async Task<(int PeriodId, int FinancialYearId)?> GetOpenPeriodByDateAsync(int companyId, DateOnly date)
        {
            const string sql = @"
                SELECT TOP 1 ap.Id AS PeriodId, ap.FinancialYearId
                FROM Finance.AccountingPeriod ap
                INNER JOIN Finance.MiscMaster mm ON mm.Id = ap.StatusId AND mm.IsDeleted = 0
                INNER JOIN Finance.MiscTypeMaster mt ON mt.Id = mm.MiscTypeId AND mt.IsDeleted = 0
                WHERE ap.CompanyId = @CompanyId AND ap.IsDeleted = 0
                    AND @Date BETWEEN ap.StartDate AND ap.EndDate
                    AND mt.MiscTypeCode = 'PERIOD_STATUS' AND mm.Code = 'OPEN'";

            var row = await _dbConnection.QueryFirstOrDefaultAsync<OpenPeriodRow>(sql, new { CompanyId = companyId, Date = date });
            return row == null ? null : (row.PeriodId, row.FinancialYearId);
        }

        private sealed class OpenPeriodRow
        {
            public int PeriodId { get; set; }
            public int FinancialYearId { get; set; }
        }
    }
}
