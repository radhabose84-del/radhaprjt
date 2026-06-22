using System.Data;
using Contracts.Interfaces.Lookups.Users;
using Dapper;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournal;
using FinanceManagement.Application.JournalMaster.Dto;

namespace FinanceManagement.Infrastructure.Repositories.JournalMaster.Journal
{
    public class JournalQueryRepository : IJournalQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly ICompanyLookup _companyLookup;
        private readonly IFinancialYearLookup _financialYearLookup;

        public JournalQueryRepository(
            IDbConnection dbConnection,
            ICompanyLookup companyLookup,
            IFinancialYearLookup financialYearLookup)
        {
            _dbConnection = dbConnection;
            _companyLookup = companyLookup;
            _financialYearLookup = financialYearLookup;
        }

        public async Task<(List<JournalHeaderDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm, int? companyId = null)
        {
            var whereClause = "h.IsDeleted = 0";
            if (!string.IsNullOrWhiteSpace(searchTerm))
                whereClause += " AND (h.VoucherNo LIKE @Search OR h.Narration LIKE @Search)";
            if (companyId.HasValue && companyId.Value > 0)
                whereClause += " AND h.CompanyId = @CompanyId";

            var query = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Finance.JournalHeader h
                WHERE {whereClause};

                SELECT h.Id, h.CompanyId, h.UnitId, h.VoucherTypeId, vt.VoucherTypeCode, vt.VoucherTypeName,
                    h.VoucherNo, h.VoucherDate, h.PostingDate, h.FinancialYearId, h.AccountingPeriodId, ap.PeriodName,
                    h.Narration, h.StatusId, ms.Description AS StatusName, h.SourceId, msrc.Description AS SourceName,
                    h.TriggerDocType, h.TriggerDocRef, h.AutoApproved, h.TotalDr, h.TotalCr,
                    h.ReversalOfId, h.IsReversal, h.CopiedFromRef, h.ImportBatchId,
                    h.IsActive, h.IsDeleted,
                    h.CreatedBy, h.CreatedDate, h.CreatedByName, h.CreatedIP,
                    h.ModifiedBy, h.ModifiedDate, h.ModifiedByName, h.ModifiedIP
                FROM Finance.JournalHeader h
                LEFT JOIN Finance.VoucherTypeMaster vt ON vt.Id = h.VoucherTypeId
                LEFT JOIN Finance.AccountingPeriod ap ON ap.Id = h.AccountingPeriodId
                LEFT JOIN Finance.MiscMaster ms ON ms.Id = h.StatusId
                LEFT JOIN Finance.MiscMaster msrc ON msrc.Id = h.SourceId
                WHERE {whereClause}
                ORDER BY h.VoucherDate DESC, h.Id DESC
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
            var list = (await multi.ReadAsync<JournalHeaderDto>()).ToList();
            var totalCount = await multi.ReadFirstAsync<int>();

            if (list.Count > 0)
                await PopulateLookupNamesAsync(list);

            return (list, totalCount);
        }

        public async Task<JournalHeaderDto?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT h.Id, h.CompanyId, h.UnitId, h.VoucherTypeId, vt.VoucherTypeCode, vt.VoucherTypeName,
                    h.VoucherNo, h.VoucherDate, h.PostingDate, h.FinancialYearId, h.AccountingPeriodId, ap.PeriodName,
                    h.Narration, h.StatusId, ms.Description AS StatusName, h.SourceId, msrc.Description AS SourceName,
                    h.TriggerDocType, h.TriggerDocRef, h.AutoApproved, h.TotalDr, h.TotalCr,
                    h.ReversalOfId, rev.VoucherNo AS ReversalOfVoucherNo, h.IsReversal, h.CopiedFromRef, h.ImportBatchId,
                    h.SubmittedBy, h.SubmittedAt, h.ApprovedBy, h.ApprovedAt, h.RejectedBy, h.RejectedAt, h.RejectReason,
                    h.PostedBy, h.PostedAt,
                    h.IsActive, h.IsDeleted,
                    h.CreatedBy, h.CreatedDate, h.CreatedByName, h.CreatedIP,
                    h.ModifiedBy, h.ModifiedDate, h.ModifiedByName, h.ModifiedIP
                FROM Finance.JournalHeader h
                LEFT JOIN Finance.VoucherTypeMaster vt ON vt.Id = h.VoucherTypeId
                LEFT JOIN Finance.AccountingPeriod ap ON ap.Id = h.AccountingPeriodId
                LEFT JOIN Finance.MiscMaster ms ON ms.Id = h.StatusId
                LEFT JOIN Finance.MiscMaster msrc ON msrc.Id = h.SourceId
                LEFT JOIN Finance.JournalHeader rev ON rev.Id = h.ReversalOfId
                WHERE h.Id = @Id AND h.IsDeleted = 0;

                SELECT d.Id, d.JournalHeaderId, d.[LineNo], d.GlAccountId, ga.AccountCode AS GlAccountCode, ga.AccountName AS GlAccountName,
                    d.DrAmount, d.CrAmount, d.CurrencyId, d.ExchangeRate, d.BaseDrAmount, d.BaseCrAmount,
                    d.CostCentreId, cc.CostCentreName, d.ProfitCentreId, pc.ProfitCentreName,
                    d.LineNarration, d.ReferenceDocNo
                FROM Finance.JournalDetail d
                LEFT JOIN Finance.GlAccountMaster ga ON ga.Id = d.GlAccountId
                LEFT JOIN Finance.CostCentre cc ON cc.Id = d.CostCentreId
                LEFT JOIN Finance.ProfitCentre pc ON pc.Id = d.ProfitCentreId
                WHERE d.JournalHeaderId = @Id AND d.IsDeleted = 0
                ORDER BY d.[LineNo] ASC;";

            var multi = await _dbConnection.QueryMultipleAsync(sql, new { Id = id });
            var dto = await multi.ReadFirstOrDefaultAsync<JournalHeaderDto>();
            if (dto == null)
                return null;

            dto.Lines = (await multi.ReadAsync<JournalDetailDto>()).ToList();

            await PopulateLookupNamesAsync(new List<JournalHeaderDto> { dto });
            return dto;
        }

        private async Task PopulateLookupNamesAsync(List<JournalHeaderDto> list)
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

        public async Task<int> GetStatusIdAsync(string code) => await GetMiscIdAsync("JOURNAL_STATUS", code);

        public async Task<int> GetSourceIdAsync(string code) => await GetMiscIdAsync("JOURNAL_SOURCE", code);

        private async Task<int> GetMiscIdAsync(string typeCode, string code)
        {
            const string sql = @"
                SELECT mm.Id
                FROM Finance.MiscMaster mm
                INNER JOIN Finance.MiscTypeMaster mt ON mt.Id = mm.MiscTypeId AND mt.IsDeleted = 0
                WHERE mt.MiscTypeCode = @TypeCode AND mm.Code = @Code
                    AND mm.IsActive = 1 AND mm.IsDeleted = 0";

            return await _dbConnection.ExecuteScalarAsync<int>(sql, new { TypeCode = typeCode, Code = code });
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

        public async Task<bool> GlAccountExistsAsync(int glAccountId, int companyId)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Finance.GlAccountMaster
                    WHERE Id = @Id AND CompanyId = @CompanyId AND IsActive = 1 AND IsDeleted = 0
                ) THEN 1 ELSE 0 END";
            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = glAccountId, CompanyId = companyId });
        }

        public async Task<IReadOnlyCollection<int>> GetCostCentreMandatoryAccountIdsAsync(IEnumerable<int> glAccountIds)
        {
            var ids = glAccountIds.Distinct().ToList();
            if (ids.Count == 0)
                return Array.Empty<int>();

            const string sql = @"
                SELECT Id FROM Finance.GlAccountMaster
                WHERE Id IN @Ids AND IsCostCentreMandatory = 1 AND IsDeleted = 0";
            var result = await _dbConnection.QueryAsync<int>(sql, new { Ids = ids });
            return result.ToList();
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

        public async Task<bool> CurrencyExistsAsync(int currencyId)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Finance.CurrencyForexConfig WHERE Id = @Id AND IsDeleted = 0
                ) THEN 1 ELSE 0 END";
            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = currencyId });
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1) FROM Finance.JournalHeader WHERE Id = @Id AND IsDeleted = 0";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<bool> IsDraftAsync(int id)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1
                    FROM Finance.JournalHeader h
                    INNER JOIN Finance.MiscMaster mm ON mm.Id = h.StatusId
                    INNER JOIN Finance.MiscTypeMaster mt ON mt.Id = mm.MiscTypeId
                    WHERE h.Id = @Id AND h.IsDeleted = 0
                        AND mt.MiscTypeCode = 'JOURNAL_STATUS' AND mm.Code = 'DRAFT'
                ) THEN 1 ELSE 0 END";
            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = id });
        }

        public async Task<bool> IsPostedAsync(int id)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1
                    FROM Finance.JournalHeader h
                    INNER JOIN Finance.MiscMaster mm ON mm.Id = h.StatusId
                    INNER JOIN Finance.MiscTypeMaster mt ON mt.Id = mm.MiscTypeId
                    WHERE h.Id = @Id AND h.IsDeleted = 0
                        AND mt.MiscTypeCode = 'JOURNAL_STATUS' AND mm.Code IN ('POSTED', 'REVERSED')
                ) THEN 1 ELSE 0 END";
            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = id });
        }

        public async Task<bool> IsBalancedAsync(int id)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Finance.JournalHeader
                    WHERE Id = @Id AND IsDeleted = 0 AND TotalDr = TotalCr AND TotalDr > 0
                ) THEN 1 ELSE 0 END";
            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = id });
        }

        public async Task<bool> IsPeriodOpenAsync(int id)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1
                    FROM Finance.JournalHeader h
                    INNER JOIN Finance.AccountingPeriod ap ON ap.Id = h.AccountingPeriodId AND ap.IsDeleted = 0
                    INNER JOIN Finance.MiscMaster mm ON mm.Id = ap.StatusId
                    INNER JOIN Finance.MiscTypeMaster mt ON mt.Id = mm.MiscTypeId
                    WHERE h.Id = @Id AND h.IsDeleted = 0
                        AND mt.MiscTypeCode = 'PERIOD_STATUS' AND mm.Code = 'OPEN'
                ) THEN 1 ELSE 0 END";
            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = id });
        }

        private sealed class OpenPeriodRow
        {
            public int PeriodId { get; set; }
            public int FinancialYearId { get; set; }
        }
    }
}
