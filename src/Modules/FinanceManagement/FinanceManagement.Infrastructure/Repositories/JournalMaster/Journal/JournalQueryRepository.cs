using System.Data;
using System.Globalization;
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

        public async Task<(List<JournalHeaderDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm, int? companyId = null, int? statusId = null)
        {
            var whereClause = "h.IsDeleted = 0";
            if (!string.IsNullOrWhiteSpace(searchTerm))
                whereClause += " AND (h.VoucherNo LIKE @Search OR h.Narration LIKE @Search)";
            if (companyId.HasValue && companyId.Value > 0)
                whereClause += " AND h.CompanyId = @CompanyId";
            if (statusId.HasValue && statusId.Value > 0)
                whereClause += " AND h.StatusId = @StatusId";

            var query = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Finance.JournalHeader h
                WHERE {whereClause};

                SELECT h.Id, h.CompanyId, h.UnitId, h.VoucherTypeId, vt.VoucherTypeCode, vt.VoucherTypeName,
                    h.VoucherNo, h.VoucherDate, h.PostingDate, h.FinancialYearId, h.AccountingPeriodId, ap.PeriodName,
                    h.Narration, pl.AccountCode, pl.AccountName,
                    h.StatusId, ms.Description AS StatusName, h.SourceId, msrc.Description AS SourceName,
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
                OUTER APPLY (
                    SELECT TOP 1 ga.AccountCode, ga.AccountName
                    FROM Finance.JournalDetail d
                    LEFT JOIN Finance.GlAccountMaster ga ON ga.Id = d.GlAccountId
                    WHERE d.JournalHeaderId = h.Id AND d.IsDeleted = 0
                    ORDER BY CASE WHEN d.DrAmount > 0 THEN 0 ELSE 1 END, d.[LineNo]
                ) pl
                WHERE {whereClause}
                ORDER BY h.VoucherDate DESC, h.Id DESC
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

            var multi = await _dbConnection.QueryMultipleAsync(query, parameters);
            var list = (await multi.ReadAsync<JournalHeaderDto>()).ToList();
            var totalCount = await multi.ReadFirstAsync<int>();

            if (list.Count > 0)
                await PopulateLookupNamesAsync(list);

            return (list, totalCount);
        }

        public async Task<(List<JournalListItemDto>, int)> SearchAsync(JournalSearchFilter filter, int pageNumber, int pageSize, int? companyId = null)
        {
            var where = new List<string> { "h.IsDeleted = 0" };
            var p = new DynamicParameters();

            if (companyId is > 0) { where.Add("h.CompanyId = @CompanyId"); p.Add("CompanyId", companyId); }
            if (!string.IsNullOrWhiteSpace(filter.VoucherNo)) { where.Add("h.VoucherNo LIKE @VoucherNo"); p.Add("VoucherNo", $"%{filter.VoucherNo}%"); }
            if (filter.DateFrom.HasValue) { where.Add("h.VoucherDate >= @DateFrom"); p.Add("DateFrom", filter.DateFrom.Value); }
            if (filter.DateTo.HasValue) { where.Add("h.VoucherDate <= @DateTo"); p.Add("DateTo", filter.DateTo.Value); }
            if (filter.VoucherTypeId is > 0) { where.Add("h.VoucherTypeId = @VoucherTypeId"); p.Add("VoucherTypeId", filter.VoucherTypeId); }
            if (filter.StatusId is > 0) { where.Add("h.StatusId = @StatusId"); p.Add("StatusId", filter.StatusId); }
            if (filter.SourceId is > 0) { where.Add("h.SourceId = @SourceId"); p.Add("SourceId", filter.SourceId); }
            if (filter.CreatedBy is > 0) { where.Add("h.CreatedBy = @CreatedBy"); p.Add("CreatedBy", filter.CreatedBy); }
            if (!string.IsNullOrWhiteSpace(filter.ApprovedBy)) { where.Add("h.ApprovedBy LIKE @ApprovedBy"); p.Add("ApprovedBy", $"%{filter.ApprovedBy}%"); }
            if (filter.AmountMin.HasValue) { where.Add("h.TotalDr >= @AmountMin"); p.Add("AmountMin", filter.AmountMin.Value); }
            if (filter.AmountMax.HasValue) { where.Add("h.TotalDr <= @AmountMax"); p.Add("AmountMax", filter.AmountMax.Value); }
            if (!string.IsNullOrWhiteSpace(filter.Narration)) { where.Add("h.Narration LIKE @Narration"); p.Add("Narration", $"%{filter.Narration}%"); }
            if (filter.AccountId is > 0)
            {
                where.Add("EXISTS (SELECT 1 FROM Finance.JournalDetail d WHERE d.JournalHeaderId = h.Id AND d.IsDeleted = 0 AND d.GlAccountId = @AccountId)");
                p.Add("AccountId", filter.AccountId);
            }
            if (filter.CostCentreId is > 0)
            {
                where.Add("EXISTS (SELECT 1 FROM Finance.JournalDetail d WHERE d.JournalHeaderId = h.Id AND d.IsDeleted = 0 AND d.CostCentreId = @CostCentreId)");
                p.Add("CostCentreId", filter.CostCentreId);
            }
            if (!string.IsNullOrWhiteSpace(filter.Reference))
            {
                where.Add(@"(h.TriggerDocRef LIKE @Reference
                    OR EXISTS (SELECT 1 FROM Finance.JournalDetail d WHERE d.JournalHeaderId = h.Id AND d.IsDeleted = 0 AND d.ReferenceDocNo LIKE @Reference))");
                p.Add("Reference", $"%{filter.Reference}%");
            }

            var whereClause = string.Join(" AND ", where);
            p.Add("Offset", (pageNumber - 1) * pageSize);
            p.Add("PageSize", pageSize);

            var query = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*) FROM Finance.JournalHeader h WHERE {whereClause};

                SELECT h.Id, h.VoucherNo, h.VoucherDate, vt.VoucherTypeCode,
                    pl.GlAccountId, pl.AccountCode, pl.AccountName, pl.CostCentreCode, pl.CostCentreName,
                    h.TotalDr, h.TotalCr, h.Narration,
                    h.StatusId, ms.Description AS StatusName, h.SourceId, msrc.Description AS SourceName,
                    h.CreatedBy, h.CreatedByName AS CreatorName, h.ApprovedBy AS ApproverName, h.ApprovedAt,
                    COALESCE(pl.ReferenceDocNo, h.TriggerDocRef) AS Reference
                FROM Finance.JournalHeader h
                LEFT JOIN Finance.VoucherTypeMaster vt ON vt.Id = h.VoucherTypeId
                LEFT JOIN Finance.MiscMaster ms ON ms.Id = h.StatusId
                LEFT JOIN Finance.MiscMaster msrc ON msrc.Id = h.SourceId
                OUTER APPLY (
                    SELECT TOP 1 d.GlAccountId, ga.AccountCode, ga.AccountName,
                        cc.CostCentreCode, cc.CostCentreName, d.ReferenceDocNo
                    FROM Finance.JournalDetail d
                    LEFT JOIN Finance.GlAccountMaster ga ON ga.Id = d.GlAccountId
                    LEFT JOIN Finance.CostCentre cc ON cc.Id = d.CostCentreId
                    WHERE d.JournalHeaderId = h.Id AND d.IsDeleted = 0
                    ORDER BY CASE WHEN d.DrAmount > 0 THEN 0 ELSE 1 END, d.[LineNo]
                ) pl
                WHERE {whereClause}
                ORDER BY h.VoucherDate DESC, h.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;";

            var multi = await _dbConnection.QueryMultipleAsync(query, p);
            var list = (await multi.ReadAsync<JournalListItemDto>()).ToList();
            var totalCount = await multi.ReadFirstAsync<int>();

            return (list, totalCount);
        }

        public async Task<(List<JournalListItemDto>, int)> GetPostableAsync(int pageNumber, int pageSize, int? companyId = null)
        {
            // Mirrors IsPostingEligibleAsync as a list: APPROVED, or a system journal (source != MANUAL) still in DRAFT.
            var p = new DynamicParameters();
            p.Add("Offset", (pageNumber - 1) * pageSize);
            p.Add("PageSize", pageSize);

            var companyFilter = string.Empty;
            if (companyId is > 0) { companyFilter = " AND h.CompanyId = @CompanyId"; p.Add("CompanyId", companyId); }

            var eligibility = @"
                EXISTS (
                    SELECT 1
                    FROM Finance.MiscMaster ms2
                    INNER JOIN Finance.MiscTypeMaster mts2 ON mts2.Id = ms2.MiscTypeId AND mts2.MiscTypeCode = 'JOURNAL_STATUS'
                    INNER JOIN Finance.MiscMaster src2 ON src2.Id = h.SourceId
                    INNER JOIN Finance.MiscTypeMaster mtsrc2 ON mtsrc2.Id = src2.MiscTypeId AND mtsrc2.MiscTypeCode = 'JOURNAL_SOURCE'
                    WHERE ms2.Id = h.StatusId
                        AND (ms2.Code = 'APPROVED' OR (ms2.Code = 'DRAFT' AND src2.Code <> 'MANUAL'))
                )";

            var whereClause = $"h.IsDeleted = 0{companyFilter} AND {eligibility}";

            var query = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*) FROM Finance.JournalHeader h WHERE {whereClause};

                SELECT h.Id, h.VoucherNo, h.VoucherDate, vt.VoucherTypeCode,
                    pl.GlAccountId, pl.AccountCode, pl.AccountName, pl.CostCentreCode, pl.CostCentreName,
                    h.TotalDr, h.TotalCr, h.Narration,
                    h.StatusId, ms.Description AS StatusName, h.SourceId, msrc.Description AS SourceName,
                    h.CreatedBy, h.CreatedByName AS CreatorName, h.ApprovedBy AS ApproverName, h.ApprovedAt,
                    COALESCE(pl.ReferenceDocNo, h.TriggerDocRef) AS Reference
                FROM Finance.JournalHeader h
                LEFT JOIN Finance.VoucherTypeMaster vt ON vt.Id = h.VoucherTypeId
                LEFT JOIN Finance.MiscMaster ms ON ms.Id = h.StatusId
                LEFT JOIN Finance.MiscMaster msrc ON msrc.Id = h.SourceId
                OUTER APPLY (
                    SELECT TOP 1 d.GlAccountId, ga.AccountCode, ga.AccountName,
                        cc.CostCentreCode, cc.CostCentreName, d.ReferenceDocNo
                    FROM Finance.JournalDetail d
                    LEFT JOIN Finance.GlAccountMaster ga ON ga.Id = d.GlAccountId
                    LEFT JOIN Finance.CostCentre cc ON cc.Id = d.CostCentreId
                    WHERE d.JournalHeaderId = h.Id AND d.IsDeleted = 0
                    ORDER BY CASE WHEN d.DrAmount > 0 THEN 0 ELSE 1 END, d.[LineNo]
                ) pl
                WHERE {whereClause}
                ORDER BY h.VoucherDate DESC, h.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;";

            var multi = await _dbConnection.QueryMultipleAsync(query, p);
            var list = (await multi.ReadAsync<JournalListItemDto>()).ToList();
            var totalCount = await multi.ReadFirstAsync<int>();

            return (list, totalCount);
        }

        public async Task<(List<JournalListItemDto>, int)> GetPendingApprovalAsync(int pageNumber, int pageSize, int? companyId = null)
        {
            // Candidates awaiting approval: manual journals (source = MANUAL) still in DRAFT.
            var p = new DynamicParameters();
            p.Add("Offset", (pageNumber - 1) * pageSize);
            p.Add("PageSize", pageSize);

            var companyFilter = string.Empty;
            if (companyId is > 0) { companyFilter = " AND h.CompanyId = @CompanyId"; p.Add("CompanyId", companyId); }

            var candidate = @"
                EXISTS (
                    SELECT 1
                    FROM Finance.MiscMaster ms2
                    INNER JOIN Finance.MiscTypeMaster mts2 ON mts2.Id = ms2.MiscTypeId AND mts2.MiscTypeCode = 'JOURNAL_STATUS'
                    INNER JOIN Finance.MiscMaster src2 ON src2.Id = h.SourceId
                    INNER JOIN Finance.MiscTypeMaster mtsrc2 ON mtsrc2.Id = src2.MiscTypeId AND mtsrc2.MiscTypeCode = 'JOURNAL_SOURCE'
                    WHERE ms2.Id = h.StatusId AND ms2.Code = 'DRAFT' AND src2.Code = 'MANUAL'
                )";

            var whereClause = $"h.IsDeleted = 0{companyFilter} AND {candidate}";

            var query = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*) FROM Finance.JournalHeader h WHERE {whereClause};

                SELECT h.Id, h.VoucherNo, h.VoucherDate, vt.VoucherTypeCode,
                    pl.GlAccountId, pl.AccountCode, pl.AccountName, pl.CostCentreCode, pl.CostCentreName,
                    h.TotalDr, h.TotalCr, h.Narration,
                    h.StatusId, ms.Description AS StatusName, h.SourceId, msrc.Description AS SourceName,
                    h.CreatedBy, h.CreatedByName AS CreatorName, h.ApprovedBy AS ApproverName, h.ApprovedAt,
                    COALESCE(pl.ReferenceDocNo, h.TriggerDocRef) AS Reference
                FROM Finance.JournalHeader h
                LEFT JOIN Finance.VoucherTypeMaster vt ON vt.Id = h.VoucherTypeId
                LEFT JOIN Finance.MiscMaster ms ON ms.Id = h.StatusId
                LEFT JOIN Finance.MiscMaster msrc ON msrc.Id = h.SourceId
                OUTER APPLY (
                    SELECT TOP 1 d.GlAccountId, ga.AccountCode, ga.AccountName,
                        cc.CostCentreCode, cc.CostCentreName, d.ReferenceDocNo
                    FROM Finance.JournalDetail d
                    LEFT JOIN Finance.GlAccountMaster ga ON ga.Id = d.GlAccountId
                    LEFT JOIN Finance.CostCentre cc ON cc.Id = d.CostCentreId
                    WHERE d.JournalHeaderId = h.Id AND d.IsDeleted = 0
                    ORDER BY CASE WHEN d.DrAmount > 0 THEN 0 ELSE 1 END, d.[LineNo]
                ) pl
                WHERE {whereClause}
                ORDER BY h.VoucherDate DESC, h.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;";

            var multi = await _dbConnection.QueryMultipleAsync(query, p);
            var list = (await multi.ReadAsync<JournalListItemDto>()).ToList();
            var totalCount = await multi.ReadFirstAsync<int>();

            return (list, totalCount);
        }

        public async Task<bool> IsManualDraftAsync(int id)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1
                    FROM Finance.JournalHeader h
                    INNER JOIN Finance.MiscMaster ms ON ms.Id = h.StatusId
                    INNER JOIN Finance.MiscTypeMaster mts ON mts.Id = ms.MiscTypeId AND mts.MiscTypeCode = 'JOURNAL_STATUS'
                    INNER JOIN Finance.MiscMaster src ON src.Id = h.SourceId
                    INNER JOIN Finance.MiscTypeMaster mtsrc ON mtsrc.Id = src.MiscTypeId AND mtsrc.MiscTypeCode = 'JOURNAL_SOURCE'
                    WHERE h.Id = @Id AND h.IsDeleted = 0 AND ms.Code = 'DRAFT' AND src.Code = 'MANUAL'
                ) THEN 1 ELSE 0 END";
            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = id });
        }

        public async Task<int> GetUnitIdAsync(int id)
        {
            const string sql = "SELECT TOP 1 UnitId FROM Finance.JournalHeader WHERE Id = @Id AND IsDeleted = 0";
            return await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
        }

        public async Task<JournalHeaderDto?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT h.Id, h.CompanyId, h.UnitId, h.VoucherTypeId, vt.VoucherTypeCode, vt.VoucherTypeName,
                    h.VoucherNo, h.VoucherDate, h.PostingDate, h.FinancialYearId, h.AccountingPeriodId, ap.PeriodName,
                    h.Narration, h.StatusId, ms.Description AS StatusName, h.SourceId, msrc.Description AS SourceName,
                    h.TriggerDocType, h.TriggerDocRef, h.AutoApproved, h.TotalDr, h.TotalCr,
                    h.ReversalOfId, rev.VoucherNo AS ReversalOfVoucherNo, h.IsReversal, h.CopiedFromRef, h.ImportBatchId,
                    h.ApprovedBy, h.ApprovedAt, h.RejectedBy, h.RejectedAt, h.RejectReason,
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

        public async Task<IReadOnlyList<JournalLookupDto>> AutocompleteAsync(string term, int? companyId, int? statusId, CancellationToken ct)
        {
            var where = new List<string> { "h.IsDeleted = 0", "h.IsActive = 1" };
            if (!string.IsNullOrWhiteSpace(term))
                where.Add("(h.VoucherNo LIKE @Term OR h.Narration LIKE @Term)");
            if (companyId is > 0) where.Add("h.CompanyId = @CompanyId");
            if (statusId is > 0) where.Add("h.StatusId = @StatusId");

            var sql = $@"
                SELECT TOP 20 h.Id, h.VoucherNo, h.VoucherDate, h.StatusId, ms.Description AS StatusName, h.TotalDr,
                    CAST(CASE WHEN ms.Code = 'POSTED' AND h.IsReversal = 0
                              AND NOT EXISTS (SELECT 1 FROM Finance.JournalHeader r WHERE r.ReversalOfId = h.Id AND r.IsDeleted = 0)
                         THEN 1 ELSE 0 END AS bit) AS IsReverseApplicable
                FROM Finance.JournalHeader h
                LEFT JOIN Finance.MiscMaster ms ON ms.Id = h.StatusId
                WHERE {string.Join(" AND ", where)}
                ORDER BY h.VoucherDate DESC, h.Id DESC";

            var result = await _dbConnection.QueryAsync<JournalLookupDto>(
                new CommandDefinition(sql, new { Term = $"%{term}%", CompanyId = companyId, StatusId = statusId }, cancellationToken: ct));
            return result.ToList();
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

        public async Task<bool> IsPotentialDuplicateAsync(
            int companyId, int voucherTypeId, DateOnly voucherDate, decimal totalDr, decimal totalCr,
            IReadOnlyList<(int GlAccountId, decimal DrAmount, decimal CrAmount)> lines, int? excludeId)
        {
            // 1) Candidate headers: same company/type/date and equal totals (cheap, index-friendly pre-filter).
            const string headerSql = @"
                SELECT Id
                FROM Finance.JournalHeader
                WHERE IsDeleted = 0
                  AND CompanyId = @CompanyId
                  AND VoucherTypeId = @VoucherTypeId
                  AND VoucherDate = @VoucherDate
                  AND TotalDr = @TotalDr
                  AND TotalCr = @TotalCr
                  AND (@ExcludeId IS NULL OR Id <> @ExcludeId)";

            var candidateIds = (await _dbConnection.QueryAsync<int>(headerSql, new
            {
                CompanyId = companyId,
                VoucherTypeId = voucherTypeId,
                VoucherDate = voucherDate,
                TotalDr = totalDr,
                TotalCr = totalCr,
                ExcludeId = excludeId
            })).ToList();

            if (candidateIds.Count == 0)
                return false;

            // 2) A candidate is a duplicate only if its line set (account + Dr + Cr) matches exactly.
            const string lineSql = @"
                SELECT JournalHeaderId, GlAccountId, DrAmount, CrAmount
                FROM Finance.JournalDetail
                WHERE IsDeleted = 0 AND JournalHeaderId IN @Ids";

            var candidateLines = (await _dbConnection.QueryAsync<DupLineRow>(
                lineSql, new { Ids = candidateIds })).ToList();

            var incomingSignature = BuildLineSignature(lines.Select(l => (l.GlAccountId, l.DrAmount, l.CrAmount)));

            return candidateLines
                .GroupBy(l => l.JournalHeaderId)
                .Any(g => BuildLineSignature(g.Select(l => (l.GlAccountId, l.DrAmount, l.CrAmount))) == incomingSignature);
        }

        private sealed class DupLineRow
        {
            public int JournalHeaderId { get; set; }
            public int GlAccountId { get; set; }
            public decimal DrAmount { get; set; }
            public decimal CrAmount { get; set; }
        }

        private static string BuildLineSignature(IEnumerable<(int GlAccountId, decimal DrAmount, decimal CrAmount)> lines) =>
            string.Join("|", lines
                .OrderBy(l => l.GlAccountId).ThenBy(l => l.DrAmount).ThenBy(l => l.CrAmount)
                // Fixed 2-decimal invariant format so DB (decimal(18,2) → "1000.00") and command ("1000") amounts match.
                .Select(l => string.Format(CultureInfo.InvariantCulture, "{0}:{1:0.00}:{2:0.00}", l.GlAccountId, l.DrAmount, l.CrAmount)));

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

        public async Task<bool> IsPostingEligibleAsync(int id)
        {
            // Postable when APPROVED, or when a system journal (source != MANUAL) is still DRAFT (bypass approval, US-07).
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1
                    FROM Finance.JournalHeader h
                    INNER JOIN Finance.MiscMaster ms ON ms.Id = h.StatusId
                    INNER JOIN Finance.MiscTypeMaster mts ON mts.Id = ms.MiscTypeId AND mts.MiscTypeCode = 'JOURNAL_STATUS'
                    INNER JOIN Finance.MiscMaster src ON src.Id = h.SourceId
                    INNER JOIN Finance.MiscTypeMaster mtsrc ON mtsrc.Id = src.MiscTypeId AND mtsrc.MiscTypeCode = 'JOURNAL_SOURCE'
                    WHERE h.Id = @Id AND h.IsDeleted = 0
                        AND (ms.Code = 'APPROVED' OR (ms.Code = 'DRAFT' AND src.Code <> 'MANUAL'))
                ) THEN 1 ELSE 0 END";
            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = id });
        }

        public async Task<bool> IsReversedAsync(int id)
        {
            // Already reversed if the status is REVERSED OR a (non-deleted) reversal mirror already points back
            // to it — the latter guards against orphan/legacy rows whose status flip didn't persist.
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1
                    FROM Finance.JournalHeader h
                    INNER JOIN Finance.MiscMaster mm ON mm.Id = h.StatusId
                    INNER JOIN Finance.MiscTypeMaster mt ON mt.Id = mm.MiscTypeId
                    WHERE h.Id = @Id AND h.IsDeleted = 0
                        AND mt.MiscTypeCode = 'JOURNAL_STATUS' AND mm.Code = 'REVERSED'
                ) OR EXISTS (
                    SELECT 1 FROM Finance.JournalHeader r
                    WHERE r.ReversalOfId = @Id AND r.IsDeleted = 0
                ) THEN 1 ELSE 0 END";
            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = id });
        }

        public async Task<bool> IsReversalAsync(int id)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Finance.JournalHeader
                    WHERE Id = @Id AND IsDeleted = 0 AND IsReversal = 1
                ) THEN 1 ELSE 0 END";
            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = id });
        }

        public async Task<DateOnly?> GetPostingDateAsync(int id)
        {
            // Read as DateTime? — Dapper's scalar path returns a boxed DateTime for a SQL 'date' column and
            // cannot cast it directly to DateOnly?.
            const string sql = "SELECT PostingDate FROM Finance.JournalHeader WHERE Id = @Id AND IsDeleted = 0";
            var value = await _dbConnection.ExecuteScalarAsync<DateTime?>(sql, new { Id = id });
            return value.HasValue ? DateOnly.FromDateTime(value.Value) : null;
        }

        public async Task<DateOnly?> GetNextOpenPeriodStartAsync(int companyId, DateOnly afterDate)
        {
            const string sql = @"
                SELECT TOP 1 ap.StartDate
                FROM Finance.AccountingPeriod ap
                INNER JOIN Finance.MiscMaster mm ON mm.Id = ap.StatusId AND mm.IsDeleted = 0
                INNER JOIN Finance.MiscTypeMaster mt ON mt.Id = mm.MiscTypeId AND mt.IsDeleted = 0
                WHERE ap.CompanyId = @CompanyId AND ap.IsDeleted = 0
                    AND ap.StartDate > @AfterDate
                    AND mt.MiscTypeCode = 'PERIOD_STATUS' AND mm.Code = 'OPEN'
                ORDER BY ap.StartDate ASC";
            var value = await _dbConnection.ExecuteScalarAsync<DateTime?>(sql, new { CompanyId = companyId, AfterDate = afterDate });
            return value.HasValue ? DateOnly.FromDateTime(value.Value) : null;
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
