using System.Data;
using Contracts.Interfaces;
using Dapper;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IRecurringJournalTemplate;
using FinanceManagement.Application.JournalMaster.Dto;

namespace FinanceManagement.Infrastructure.Repositories.JournalMaster.RecurringJournalTemplate
{
    public class RecurringJournalTemplateQueryRepository : IRecurringJournalTemplateQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IIPAddressService _ipAddressService;

        public RecurringJournalTemplateQueryRepository(IDbConnection dbConnection, IIPAddressService ipAddressService)
        {
            _dbConnection = dbConnection;
            _ipAddressService = ipAddressService;
        }

        public async Task<(List<RecurringJournalTemplateHeaderDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            // Company-scoped to the logged-in company (from the token), so the grid only shows this company's templates.
            var companyId = _ipAddressService.GetCompanyId();

            var whereClause = "t.IsDeleted = 0";
            if (!string.IsNullOrWhiteSpace(searchTerm))
                whereClause += " AND t.TemplateName LIKE @Search";
            if (companyId is > 0)
                whereClause += " AND t.CompanyId = @CompanyId";

            var query = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Finance.RecurringJournalTemplateHeader t
                WHERE {whereClause};

                SELECT t.Id, t.TemplateName, t.CompanyId, t.UnitId, t.VoucherTypeId, vt.VoucherTypeCode, vt.VoucherTypeName,
                    t.FrequencyId, mf.Description AS FrequencyName,
                    t.StartDate, t.EndDate, t.AutoPost, t.AmountAdjustmentRuleId, ma.Description AS AmountAdjustmentRuleName, t.LowRisk,
                    t.StatusId, ms.Description AS StatusName,
                    t.IsActive, t.IsDeleted,
                    t.CreatedBy, t.CreatedDate, t.CreatedByName, t.CreatedIP,
                    t.ModifiedBy, t.ModifiedDate, t.ModifiedByName, t.ModifiedIP
                FROM Finance.RecurringJournalTemplateHeader t
                LEFT JOIN Finance.VoucherTypeMaster vt ON vt.Id = t.VoucherTypeId
                LEFT JOIN Finance.MiscMaster mf ON mf.Id = t.FrequencyId
                LEFT JOIN Finance.MiscMaster ma ON ma.Id = t.AmountAdjustmentRuleId
                LEFT JOIN Finance.MiscMaster ms ON ms.Id = t.StatusId
                WHERE {whereClause}
                ORDER BY t.TemplateName ASC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;
            ";

            var parameters = new
            {
                Search = $"%{searchTerm}%",
                CompanyId = companyId,
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
                SELECT t.Id, t.TemplateName, t.CompanyId, t.UnitId, t.VoucherTypeId, vt.VoucherTypeCode, vt.VoucherTypeName,
                    t.FrequencyId, mf.Description AS FrequencyName,
                    t.StartDate, t.EndDate, t.AutoPost, t.AmountAdjustmentRuleId, ma.Description AS AmountAdjustmentRuleName, t.LowRisk,
                    t.StatusId, ms.Description AS StatusName,
                    t.IsActive, t.IsDeleted,
                    t.CreatedBy, t.CreatedDate, t.CreatedByName, t.CreatedIP,
                    t.ModifiedBy, t.ModifiedDate, t.ModifiedByName, t.ModifiedIP
                FROM Finance.RecurringJournalTemplateHeader t
                LEFT JOIN Finance.VoucherTypeMaster vt ON vt.Id = t.VoucherTypeId
                LEFT JOIN Finance.MiscMaster mf ON mf.Id = t.FrequencyId
                LEFT JOIN Finance.MiscMaster ma ON ma.Id = t.AmountAdjustmentRuleId
                LEFT JOIN Finance.MiscMaster ms ON ms.Id = t.StatusId
                WHERE t.Id = @Id AND t.IsDeleted = 0;

                SELECT d.Id, d.TemplateId, d.[LineNo], d.GlAccountId, ga.AccountCode AS GlAccountCode, ga.AccountName AS GlAccountName,
                    d.DrAmount, d.CrAmount, d.AmountFormula, d.CurrencyId, d.ExchangeRate,
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
                SELECT t.Id, t.TemplateName, t.StatusId, ms.Description AS StatusName
                FROM Finance.RecurringJournalTemplateHeader t
                LEFT JOIN Finance.MiscMaster ms ON ms.Id = t.StatusId
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

        public async Task<int> GetApprovalStatusIdAsync(string code)
        {
            const string sql = @"
                SELECT mm.Id
                FROM Finance.MiscMaster mm
                INNER JOIN Finance.MiscTypeMaster mt ON mt.Id = mm.MiscTypeId AND mt.MiscTypeCode = 'ApprovalStatus'
                WHERE mm.Code = @Code AND mm.IsActive = 1 AND mm.IsDeleted = 0";
            return await _dbConnection.ExecuteScalarAsync<int>(sql, new { Code = code });
        }

        public async Task<(List<RecurringJournalTemplateHeaderDto>, int)> GetPendingApprovalAsync(int pageNumber, int pageSize)
        {
            const string statusFilter = @"
                EXISTS (SELECT 1 FROM Finance.MiscMaster s
                        INNER JOIN Finance.MiscTypeMaster st ON st.Id = s.MiscTypeId AND st.MiscTypeCode = 'ApprovalStatus'
                        WHERE s.Id = t.StatusId AND s.Code = 'Pending')";

            var query = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*) FROM Finance.RecurringJournalTemplateHeader t
                WHERE t.IsDeleted = 0 AND {statusFilter};

                SELECT t.Id, t.TemplateName, t.CompanyId, t.UnitId, t.VoucherTypeId, vt.VoucherTypeCode, vt.VoucherTypeName,
                    t.FrequencyId, mf.Description AS FrequencyName,
                    t.StartDate, t.EndDate, t.AutoPost, t.AmountAdjustmentRuleId, ma.Description AS AmountAdjustmentRuleName, t.LowRisk,
                    t.StatusId, ms.Description AS StatusName,
                    t.IsActive, t.IsDeleted,
                    t.CreatedBy, t.CreatedDate, t.CreatedByName, t.CreatedIP,
                    t.ModifiedBy, t.ModifiedDate, t.ModifiedByName, t.ModifiedIP
                FROM Finance.RecurringJournalTemplateHeader t
                LEFT JOIN Finance.VoucherTypeMaster vt ON vt.Id = t.VoucherTypeId
                LEFT JOIN Finance.MiscMaster mf ON mf.Id = t.FrequencyId
                LEFT JOIN Finance.MiscMaster ma ON ma.Id = t.AmountAdjustmentRuleId
                LEFT JOIN Finance.MiscMaster ms ON ms.Id = t.StatusId
                WHERE t.IsDeleted = 0 AND {statusFilter}
                ORDER BY t.CreatedDate DESC, t.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;";

            var multi = await _dbConnection.QueryMultipleAsync(query, new { Offset = (pageNumber - 1) * pageSize, PageSize = pageSize });
            var list = (await multi.ReadAsync<RecurringJournalTemplateHeaderDto>()).ToList();
            var totalCount = await multi.ReadFirstAsync<int>();
            return (list, totalCount);
        }

        public async Task<RecurringTemplateScheduleInfoDto?> GetScheduleInfoAsync(int templateId)
        {
            // Returns the row regardless of soft-delete state — the scheduler/job decide via IsDeleted (and the
            // other flags), so a deleted template surfaces here and its job is removed/skipped explicitly.
            const string sql = @"
                SELECT t.Id, t.TemplateName, t.CompanyId, mf.Code AS FrequencyCode, t.StartDate, t.EndDate,
                       t.AutoPost, t.LowRisk, ms.Code AS StatusCode, t.IsActive, t.IsDeleted
                FROM Finance.RecurringJournalTemplateHeader t
                LEFT JOIN Finance.MiscMaster mf ON mf.Id = t.FrequencyId
                LEFT JOIN Finance.MiscMaster ms ON ms.Id = t.StatusId
                WHERE t.Id = @Id";
            return await _dbConnection.QueryFirstOrDefaultAsync<RecurringTemplateScheduleInfoDto>(sql, new { Id = templateId });
        }

        public async Task<(List<RecurringGeneratedInstanceDto>, int)> GetGeneratedInstancesAsync(int pageNumber, int pageSize)
        {
            // Company-scoped to the logged-in company (from the token).
            var companyId = _ipAddressService.GetCompanyId();

            // Journal-centric: ALL journal vouchers whose Source is RECURRING (enriched with the originating
            // template + period via the generation log when present).
            var whereClause = "h.IsDeleted = 0 AND msrc.Code = 'RECURRING'";
            if (companyId is > 0)
                whereClause += " AND h.CompanyId = @CompanyId";

            var query = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Finance.JournalHeader h
                INNER JOIN Finance.MiscMaster msrc ON msrc.Id = h.SourceId
                WHERE {whereClause};

                SELECT h.Id, h.Id AS GeneratedVoucherId, h.VoucherNo,
                       ISNULL(g.TemplateId, 0) AS TemplateId,
                       ISNULL(t.TemplateName, h.TriggerDocRef) AS TemplateName,
                       CAST(h.AccountingPeriodId AS varchar(20)) AS Period, ap.PeriodName,
                       h.TotalDr AS Amount,
                       ISNULL(g.GeneratedAt, h.CreatedDate) AS GeneratedAt,
                       ISNULL(g.AutoPosted, 0) AS AutoPosted,
                       h.StatusId, ms.Description AS StatusName
                FROM Finance.JournalHeader h
                INNER JOIN Finance.MiscMaster msrc ON msrc.Id = h.SourceId
                LEFT JOIN Finance.RecurringGenerationLog g ON g.GeneratedVoucherId = h.Id
                LEFT JOIN Finance.RecurringJournalTemplateHeader t ON t.Id = g.TemplateId
                LEFT JOIN Finance.AccountingPeriod ap ON ap.Id = h.AccountingPeriodId
                LEFT JOIN Finance.MiscMaster ms ON ms.Id = h.StatusId
                WHERE {whereClause}
                ORDER BY h.VoucherDate DESC, h.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;
            ";

            var parameters = new
            {
                CompanyId = companyId,
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            var multi = await _dbConnection.QueryMultipleAsync(query, parameters);
            var list = (await multi.ReadAsync<RecurringGeneratedInstanceDto>()).ToList();
            var totalCount = await multi.ReadFirstAsync<int>();
            return (list, totalCount);
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
