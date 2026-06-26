using System.Data;
using Dapper;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.ILogAnalysis;
using FinanceManagement.Application.JournalMaster.Dto;

namespace FinanceManagement.Infrastructure.Repositories.JournalMaster.LogAnalysis
{
    public class LogAnalysisQueryRepository : ILogAnalysisQueryRepository
    {
        private readonly IDbConnection _dbConnection;

        public LogAnalysisQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        // Normalized UNION across the four log sources → (LogType, Id, OccurredAt, Reference, Summary, Detail, Flag).
        private const string UnionCte = @"
            WITH logs AS (
                SELECT 'SecurityViolation' AS LogType, svl.Id AS Id, svl.AttemptedAt AS OccurredAt,
                    COALESCE(jh1.VoucherNo, CONCAT('JV#', svl.JournalHeaderId)) AS Reference,
                    CONCAT(svl.TableName, ' ', svl.AttemptedAction, ' blocked') AS Summary,
                    CONCAT('User ', svl.UserName, ' via ', svl.Channel) AS Detail,
                    CAST(1 AS bit) AS Flag
                FROM Finance.SecurityViolationLog svl
                LEFT JOIN Finance.JournalHeader jh1 ON jh1.Id = svl.JournalHeaderId
                UNION ALL
                SELECT 'SequenceGap', sg.Id, sg.ScannedAt,
                    CONCAT('Series#', sg.SeriesId),
                    CONCAT(sg.GapsFound, ' gap(s) found'),
                    sg.GapNumbers,
                    sg.Alerted
                FROM Finance.SequenceGapScanLog sg
                UNION ALL
                SELECT 'RecurringGeneration', rg.Id, rg.GeneratedAt,
                    COALESCE(jh2.VoucherNo, rt.TemplateName),
                    CONCAT('Template ', rt.TemplateName, ' generated for ', rg.Period),
                    CASE WHEN rg.AutoPosted = 1 THEN 'Auto-posted' ELSE 'Draft' END,
                    rg.AutoPosted
                FROM Finance.RecurringGenerationLog rg
                LEFT JOIN Finance.RecurringJournalTemplateHeader rt ON rt.Id = rg.TemplateId
                LEFT JOIN Finance.JournalHeader jh2 ON jh2.Id = rg.GeneratedVoucherId
                UNION ALL
                SELECT 'JournalFlag', jf.Id, jf.FlaggedAt,
                    jh3.VoucherNo,
                    CONCAT(mm.Description, ' flagged'),
                    CONCAT('Value: ', jf.Value),
                    jf.DigestSent
                FROM Finance.JournalFlag jf
                LEFT JOIN Finance.JournalHeader jh3 ON jh3.Id = jf.JournalHeaderId
                LEFT JOIN Finance.MiscMaster mm ON mm.Id = jf.RuleTypeId
            )";

        public async Task<(List<LogAnalysisDto>, int)> GetAllAsync(
            string? logType, DateTimeOffset? from, DateTimeOffset? to, int pageNumber, int pageSize)
        {
            var p = new DynamicParameters();
            p.Add("LogType", string.IsNullOrWhiteSpace(logType) ? null : logType);
            p.Add("From", from);
            p.Add("To", to);
            p.Add("Offset", (pageNumber - 1) * pageSize);
            p.Add("PageSize", pageSize);

            const string filter = @"
                WHERE (@LogType IS NULL OR LogType = @LogType)
                  AND (@From IS NULL OR OccurredAt >= @From)
                  AND (@To   IS NULL OR OccurredAt <= @To)";

            var sql = $@"
                {UnionCte}
                SELECT LogType, Id, OccurredAt, Reference, Summary, Detail, Flag, COUNT(*) OVER() AS TotalCount
                FROM logs
                {filter}
                ORDER BY OccurredAt DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

            var rows = (await _dbConnection.QueryAsync<Row>(sql, p)).ToList();
            var total = rows.Count > 0 ? rows[0].TotalCount : 0;
            var list = rows.Select(r => new LogAnalysisDto
            {
                LogType = r.LogType, Id = r.Id, OccurredAt = r.OccurredAt,
                Reference = r.Reference, Summary = r.Summary, Detail = r.Detail, Flag = r.Flag
            }).ToList();

            return (list, total);
        }

        public async Task<LogAnalysisSummaryDto> GetSummaryAsync(DateTimeOffset? from, DateTimeOffset? to)
        {
            var p = new DynamicParameters();
            p.Add("From", from);
            p.Add("To", to);

            var sql = $@"
                {UnionCte}
                SELECT LogType, COUNT(*) AS Cnt
                FROM logs
                WHERE (@From IS NULL OR OccurredAt >= @From) AND (@To IS NULL OR OccurredAt <= @To)
                GROUP BY LogType;";

            var counts = (await _dbConnection.QueryAsync<CountRow>(sql, p)).ToList();
            int Get(string t) => counts.FirstOrDefault(c => c.LogType == t)?.Cnt ?? 0;

            var dto = new LogAnalysisSummaryDto
            {
                SecurityViolationCount = Get("SecurityViolation"),
                SequenceGapCount = Get("SequenceGap"),
                RecurringGenerationCount = Get("RecurringGeneration"),
                JournalFlagCount = Get("JournalFlag")
            };
            dto.TotalCount = dto.SecurityViolationCount + dto.SequenceGapCount + dto.RecurringGenerationCount + dto.JournalFlagCount;
            return dto;
        }

        private sealed class Row
        {
            public string? LogType { get; set; }
            public int Id { get; set; }
            public DateTimeOffset OccurredAt { get; set; }
            public string? Reference { get; set; }
            public string? Summary { get; set; }
            public string? Detail { get; set; }
            public bool Flag { get; set; }
            public int TotalCount { get; set; }
        }

        private sealed class CountRow
        {
            public string? LogType { get; set; }
            public int Cnt { get; set; }
        }
    }
}
