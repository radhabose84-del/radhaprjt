using System.Data;
using Dapper;
using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Application.ScheduleIII.Dto;

namespace FinanceManagement.Infrastructure.Repositories.ScheduleIII
{
    public class ScheduleIIIImportQueryRepository : IScheduleIIIImportQueryRepository
    {
        private readonly IDbConnection _dbConnection;

        public ScheduleIIIImportQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<IReadOnlyList<ScheduleIIIMiscOptionDto>> GetStatementTypeOptionsAsync() =>
            await GetMiscOptionsAsync("S3_STMT_TYPE");

        public async Task<IReadOnlyList<ScheduleIIIMiscOptionDto>> GetNatureOptionsAsync() =>
            await GetMiscOptionsAsync("S3_NATURE");

        private async Task<IReadOnlyList<ScheduleIIIMiscOptionDto>> GetMiscOptionsAsync(string miscTypeCode)
        {
            const string sql = @"
                SELECT m.Id, m.Code, m.Description
                FROM Finance.MiscMaster m
                INNER JOIN Finance.MiscTypeMaster t ON t.Id = m.MiscTypeId AND t.IsDeleted = 0
                WHERE t.MiscTypeCode = @MiscTypeCode AND m.IsActive = 1 AND m.IsDeleted = 0;";

            var result = await _dbConnection.QueryAsync<ScheduleIIIMiscOptionDto>(sql, new { MiscTypeCode = miscTypeCode });
            return result.ToList();
        }

        public async Task<IReadOnlyList<string>> GetExistingSectionNamesAsync(IEnumerable<string> names)
        {
            var list = names?.Where(n => !string.IsNullOrWhiteSpace(n)).Distinct().ToList() ?? new List<string>();
            if (list.Count == 0)
                return new List<string>();

            const string sql = @"
                SELECT SectionName
                FROM Finance.ScheduleIIISection
                WHERE IsDeleted = 0 AND SectionName IN @Names;";

            var result = await _dbConnection.QueryAsync<string>(sql, new { Names = list });
            return result.ToList();
        }

        public async Task<int> GetMaxSectionDisplayOrderAsync()
        {
            const string sql = @"
                SELECT ISNULL(MAX(DisplayOrder), 0)
                FROM Finance.ScheduleIIISection
                WHERE IsDeleted = 0;";

            return await _dbConnection.ExecuteScalarAsync<int>(sql);
        }
    }
}
