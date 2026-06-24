using System.Data;
using Dapper;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IRecurringJournalTemplate;
using FinanceManagement.Application.JournalMaster.Dto;

namespace FinanceManagement.Infrastructure.Repositories.JournalMaster.RecurringJournalTemplate
{
    public class RecurringJournalTemplateQueryRepository : IRecurringJournalTemplateQueryRepository
    {
        private readonly IDbConnection _dbConnection;

        public RecurringJournalTemplateQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<(List<RecurringJournalTemplateHeaderDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var whereClause = "t.IsDeleted = 0";
            if (!string.IsNullOrWhiteSpace(searchTerm))
                whereClause += " AND t.TemplateName LIKE @Search";

            var query = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Finance.RecurringJournalTemplateHeader t
                WHERE {whereClause};

                SELECT t.Id, t.TemplateName, t.VoucherTypeId, vt.VoucherTypeCode, vt.VoucherTypeName,
                    t.FrequencyId, mf.Description AS FrequencyName,
                    t.StartDate, t.EndDate, t.AutoPost, t.AmountAdjustmentRuleId, ma.Description AS AmountAdjustmentRuleName, t.LowRisk,
                    t.IsActive, t.IsDeleted,
                    t.CreatedBy, t.CreatedDate, t.CreatedByName, t.CreatedIP,
                    t.ModifiedBy, t.ModifiedDate, t.ModifiedByName, t.ModifiedIP
                FROM Finance.RecurringJournalTemplateHeader t
                LEFT JOIN Finance.VoucherTypeMaster vt ON vt.Id = t.VoucherTypeId
                LEFT JOIN Finance.MiscMaster mf ON mf.Id = t.FrequencyId
                LEFT JOIN Finance.MiscMaster ma ON ma.Id = t.AmountAdjustmentRuleId
                WHERE {whereClause}
                ORDER BY t.TemplateName ASC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;
            ";

            var parameters = new
            {
                Search = $"%{searchTerm}%",
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            var multi = await _dbConnection.QueryMultipleAsync(query, parameters);
            var list = (await multi.ReadAsync<RecurringJournalTemplateHeaderDto>()).ToList();
            var totalCount = await multi.ReadFirstAsync<int>();
            return (list, totalCount);
        }

        public async Task<RecurringJournalTemplateHeaderDto?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT t.Id, t.TemplateName, t.VoucherTypeId, vt.VoucherTypeCode, vt.VoucherTypeName,
                    t.FrequencyId, mf.Description AS FrequencyName,
                    t.StartDate, t.EndDate, t.AutoPost, t.AmountAdjustmentRuleId, ma.Description AS AmountAdjustmentRuleName, t.LowRisk,
                    t.IsActive, t.IsDeleted,
                    t.CreatedBy, t.CreatedDate, t.CreatedByName, t.CreatedIP,
                    t.ModifiedBy, t.ModifiedDate, t.ModifiedByName, t.ModifiedIP
                FROM Finance.RecurringJournalTemplateHeader t
                LEFT JOIN Finance.VoucherTypeMaster vt ON vt.Id = t.VoucherTypeId
                LEFT JOIN Finance.MiscMaster mf ON mf.Id = t.FrequencyId
                LEFT JOIN Finance.MiscMaster ma ON ma.Id = t.AmountAdjustmentRuleId
                WHERE t.Id = @Id AND t.IsDeleted = 0;

                SELECT d.Id, d.TemplateId, d.[LineNo], d.GlAccountId, ga.AccountCode AS GlAccountCode, ga.AccountName AS GlAccountName,
                    d.DrAmount, d.CrAmount, d.AmountFormula,
                    d.CostCentreId, cc.CostCentreName, d.ProfitCentreId, pc.ProfitCentreName, d.LineNarration
                FROM Finance.RecurringJournalTemplateDetail d
                LEFT JOIN Finance.GlAccountMaster ga ON ga.Id = d.GlAccountId
                LEFT JOIN Finance.CostCentre cc ON cc.Id = d.CostCentreId
                LEFT JOIN Finance.ProfitCentre pc ON pc.Id = d.ProfitCentreId
                WHERE d.TemplateId = @Id AND d.IsDeleted = 0
                ORDER BY d.[LineNo] ASC;";

            var multi = await _dbConnection.QueryMultipleAsync(sql, new { Id = id });
            var dto = await multi.ReadFirstOrDefaultAsync<RecurringJournalTemplateHeaderDto>();
            if (dto == null)
                return null;

            dto.Lines = (await multi.ReadAsync<RecurringJournalTemplateDetailDto>()).ToList();
            return dto;
        }

        public async Task<IReadOnlyList<RecurringJournalTemplateLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            var whereClause = "t.IsDeleted = 0 AND t.IsActive = 1";
            if (!string.IsNullOrWhiteSpace(term))
                whereClause += " AND t.TemplateName LIKE @Term";

            var sql = $@"
                SELECT t.Id, t.TemplateName
                FROM Finance.RecurringJournalTemplateHeader t
                WHERE {whereClause}
                ORDER BY t.TemplateName ASC";

            var result = await _dbConnection.QueryAsync<RecurringJournalTemplateLookupDto>(
                new CommandDefinition(sql, new { Term = $"%{term}%" }, cancellationToken: ct));
            return result.ToList();
        }

        public async Task<bool> AlreadyExistsByNameAsync(string templateName, int? id = null)
        {
            var sql = @"
                SELECT COUNT(1) FROM Finance.RecurringJournalTemplateHeader
                WHERE TemplateName = @Name AND IsDeleted = 0";
            if (id.HasValue && id.Value > 0)
                sql += " AND Id != @Id";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Name = templateName.Trim(), Id = id });
            return count > 0;
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"SELECT COUNT(1) FROM Finance.RecurringJournalTemplateHeader WHERE Id = @Id AND IsDeleted = 0";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<bool> VoucherTypeExistsAsync(int voucherTypeId, int companyId)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Finance.VoucherTypeMaster
                    WHERE Id = @Id AND CompanyId = @CompanyId AND IsActive = 1 AND IsDeleted = 0
                ) THEN 1 ELSE 0 END";
            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = voucherTypeId, CompanyId = companyId });
        }

        public async Task<bool> FrequencyExistsAsync(int miscId) => await MiscExistsAsync(miscId, "RECURRING_FREQUENCY");

        public async Task<bool> AmountAdjustmentRuleExistsAsync(int miscId) => await MiscExistsAsync(miscId, "AMOUNT_ADJ_RULE");

        private async Task<bool> MiscExistsAsync(int miscId, string typeCode)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1
                    FROM Finance.MiscMaster mm
                    INNER JOIN Finance.MiscTypeMaster mt ON mt.Id = mm.MiscTypeId AND mt.IsDeleted = 0
                    WHERE mm.Id = @Id AND mm.IsActive = 1 AND mm.IsDeleted = 0 AND mt.MiscTypeCode = @TypeCode
                ) THEN 1 ELSE 0 END";
            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = miscId, TypeCode = typeCode });
        }

        public async Task<bool> GlAccountExistsAsync(int glAccountId, int companyId)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Finance.GlAccountMaster
                    WHERE Id = @Id AND CompanyId = @CompanyId AND IsActive = 1 AND IsDeleted = 0
                ) THEN 1 ELSE 0 END";
            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = glAccountId, CompanyId = companyId });
        }

        public async Task<bool> CostCentreExistsAsync(int costCentreId)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Finance.CostCentre WHERE Id = @Id AND IsActive = 1 AND IsDeleted = 0
                ) THEN 1 ELSE 0 END";
            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = costCentreId });
        }

        public async Task<bool> ProfitCentreExistsAsync(int profitCentreId)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Finance.ProfitCentre WHERE Id = @Id AND IsActive = 1 AND IsDeleted = 0
                ) THEN 1 ELSE 0 END";
            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = profitCentreId });
        }
    }
}
