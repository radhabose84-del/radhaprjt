using System.Data;
using Dapper;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.ISecurityViolationLog;
using FinanceManagement.Application.JournalMaster.Dto;

namespace FinanceManagement.Infrastructure.Repositories.JournalMaster.SecurityViolationLog
{
    public class SecurityViolationLogQueryRepository : ISecurityViolationLogQueryRepository
    {
        private readonly IDbConnection _dbConnection;

        public SecurityViolationLogQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<(List<SecurityViolationLogDto>, int)> GetAllAsync(int pageNumber, int pageSize, int? journalHeaderId)
        {
            var whereClause = "1 = 1";
            if (journalHeaderId.HasValue && journalHeaderId.Value > 0)
                whereClause += " AND s.JournalHeaderId = @JournalHeaderId";

            var query = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*) FROM Finance.SecurityViolationLog s WHERE {whereClause};

                SELECT s.Id, s.TableName, s.JournalHeaderId, s.AttemptedAction, s.UserName, s.AttemptedAt, s.Channel
                FROM Finance.SecurityViolationLog s
                WHERE {whereClause}
                ORDER BY s.AttemptedAt DESC, s.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;";

            var multi = await _dbConnection.QueryMultipleAsync(query, new { JournalHeaderId = journalHeaderId, Offset = (pageNumber - 1) * pageSize, PageSize = pageSize });
            var list = (await multi.ReadAsync<SecurityViolationLogDto>()).ToList();
            var totalCount = await multi.ReadFirstAsync<int>();
            return (list, totalCount);
        }
    }
}
