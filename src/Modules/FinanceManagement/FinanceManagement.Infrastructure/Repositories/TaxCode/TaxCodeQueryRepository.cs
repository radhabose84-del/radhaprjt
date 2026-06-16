using System.Data;
using Dapper;
using FinanceManagement.Application.Common.Interfaces.ITaxCode;
using FinanceManagement.Application.TaxCode.Dto;

namespace FinanceManagement.Infrastructure.Repositories.TaxCode
{
    public class TaxCodeQueryRepository : ITaxCodeQueryRepository
    {
        private readonly IDbConnection _dbConnection;

        public TaxCodeQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        // ─── Tax Code Master ───────────────────────────────────────────────
        private const string TaxCodeColumns = @"
            tcm.Id, tcm.CompanyId, tcm.TaxCode, tcm.TaxName, tcm.TaxType, tcm.TaxComponent,
            tcm.ParentTaxCodeId, p.TaxCode AS ParentTaxCode,
            tcm.Direction, tcm.StatutorySection, tcm.ThresholdAmount, tcm.ThresholdAggregate, tcm.HsnSacCode,
            tcm.IsSystemOnlyPosting, tcm.IsEefcRelevant, tcm.IsStatutoryFixed,
            v.RatePercent AS CurrentRatePercent, v.EffectiveFrom AS CurrentEffectiveFrom,
            tcm.IsActive, tcm.IsDeleted,
            tcm.CreatedBy, tcm.CreatedDate, tcm.CreatedByName, tcm.CreatedIP,
            tcm.ModifiedBy, tcm.ModifiedDate, tcm.ModifiedByName, tcm.ModifiedIP";

        private const string TaxCodeFromJoins = @"
            FROM Finance.TaxCodeMaster tcm
            LEFT JOIN Finance.TaxCodeMaster p ON tcm.ParentTaxCodeId = p.Id
            OUTER APPLY (
                SELECT TOP 1 v2.RatePercent, v2.EffectiveFrom
                FROM Finance.TaxCodeRateVersion v2
                WHERE v2.TaxCodeId = tcm.Id AND v2.IsDeleted = 0 AND v2.EffectiveTo IS NULL
                ORDER BY v2.VersionNo DESC) v";

        public async Task<(List<TaxCodeMasterDto>, int)> GetAllTaxCodesAsync(int pageNumber, int pageSize, string? searchTerm, int? companyId, string? taxType)
        {
            var whereClause = "tcm.IsDeleted = 0";
            if (!string.IsNullOrWhiteSpace(searchTerm))
                whereClause += " AND (tcm.TaxCode LIKE @Search OR tcm.TaxName LIKE @Search)";
            if (companyId.HasValue && companyId.Value > 0)
                whereClause += " AND tcm.CompanyId = @CompanyId";
            if (!string.IsNullOrWhiteSpace(taxType))
                whereClause += " AND tcm.TaxType = @TaxType";

            var query = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*) FROM Finance.TaxCodeMaster tcm WHERE {whereClause};

                SELECT {TaxCodeColumns}
                {TaxCodeFromJoins}
                WHERE {whereClause}
                ORDER BY tcm.CompanyId ASC, tcm.TaxType ASC, tcm.TaxCode ASC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;
            ";

            var parameters = new
            {
                Search = $"%{searchTerm}%",
                CompanyId = companyId,
                TaxType = taxType,
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            var result = await _dbConnection.QueryMultipleAsync(query, parameters);
            var list = (await result.ReadAsync<TaxCodeMasterDto>()).ToList();
            var totalCount = await result.ReadFirstAsync<int>();
            return (list, totalCount);
        }

        public async Task<TaxCodeMasterDto?> GetTaxCodeByIdAsync(int id)
        {
            var sql = $@"SELECT {TaxCodeColumns} {TaxCodeFromJoins} WHERE tcm.Id = @Id AND tcm.IsDeleted = 0";
            return await _dbConnection.QueryFirstOrDefaultAsync<TaxCodeMasterDto>(sql, new { Id = id });
        }

        public async Task<IReadOnlyList<TaxCodeMasterLookupDto>> TaxCodeAutocompleteAsync(string term, int? companyId, string? taxType, CancellationToken ct)
        {
            var whereClause = "tcm.IsDeleted = 0 AND tcm.IsActive = 1";
            if (!string.IsNullOrWhiteSpace(term))
                whereClause += " AND (tcm.TaxCode LIKE @Term OR tcm.TaxName LIKE @Term)";
            if (companyId.HasValue && companyId.Value > 0)
                whereClause += " AND tcm.CompanyId = @CompanyId";
            if (!string.IsNullOrWhiteSpace(taxType))
                whereClause += " AND tcm.TaxType = @TaxType";

            var sql = $@"
                SELECT tcm.Id, tcm.CompanyId, tcm.TaxCode, tcm.TaxName, tcm.TaxType, tcm.TaxComponent
                FROM Finance.TaxCodeMaster tcm
                WHERE {whereClause}
                ORDER BY tcm.TaxType ASC, tcm.TaxCode ASC";

            var result = await _dbConnection.QueryAsync<TaxCodeMasterLookupDto>(
                new CommandDefinition(sql, new { Term = $"%{term}%", CompanyId = companyId, TaxType = taxType }, cancellationToken: ct));
            return result.ToList();
        }

        public async Task<List<TaxCodeRateVersionDto>> GetRateVersionsAsync(int taxCodeId)
        {
            const string sql = @"
                SELECT Id, TaxCodeId, VersionNo, RatePercent, EffectiveFrom, EffectiveTo, ChangeReason,
                    IsActive, IsDeleted, CreatedBy, CreatedDate, CreatedByName
                FROM Finance.TaxCodeRateVersion
                WHERE TaxCodeId = @TaxCodeId AND IsDeleted = 0
                ORDER BY VersionNo ASC";

            var result = await _dbConnection.QueryAsync<TaxCodeRateVersionDto>(sql, new { TaxCodeId = taxCodeId });
            return result.ToList();
        }

        public async Task<TaxCodeMasterDto?> GetEffectiveAsync(string code, int? companyId, DateOnly asOf)
        {
            var whereClause = "tcm.TaxCode = @Code AND tcm.IsDeleted = 0";
            if (companyId.HasValue && companyId.Value > 0)
                whereClause += " AND tcm.CompanyId = @CompanyId";

            var sql = $@"
                SELECT tcm.Id, tcm.CompanyId, tcm.TaxCode, tcm.TaxName, tcm.TaxType, tcm.TaxComponent,
                    tcm.ParentTaxCodeId, p.TaxCode AS ParentTaxCode,
                    tcm.Direction, tcm.StatutorySection, tcm.ThresholdAmount, tcm.ThresholdAggregate, tcm.HsnSacCode,
                    tcm.IsSystemOnlyPosting, tcm.IsEefcRelevant, tcm.IsStatutoryFixed,
                    v.RatePercent AS CurrentRatePercent, v.EffectiveFrom AS CurrentEffectiveFrom,
                    tcm.IsActive, tcm.IsDeleted,
                    tcm.CreatedBy, tcm.CreatedDate, tcm.CreatedByName, tcm.CreatedIP,
                    tcm.ModifiedBy, tcm.ModifiedDate, tcm.ModifiedByName, tcm.ModifiedIP
                FROM Finance.TaxCodeMaster tcm
                LEFT JOIN Finance.TaxCodeMaster p ON tcm.ParentTaxCodeId = p.Id
                OUTER APPLY (
                    SELECT TOP 1 v2.RatePercent, v2.EffectiveFrom
                    FROM Finance.TaxCodeRateVersion v2
                    WHERE v2.TaxCodeId = tcm.Id AND v2.IsDeleted = 0
                        AND v2.EffectiveFrom <= @AsOf
                        AND (v2.EffectiveTo IS NULL OR v2.EffectiveTo >= @AsOf)
                    ORDER BY v2.EffectiveFrom DESC) v
                WHERE {whereClause}";

            return await _dbConnection.QueryFirstOrDefaultAsync<TaxCodeMasterDto>(sql, new { Code = code, CompanyId = companyId, AsOf = asOf });
        }

        public async Task<bool> TaxCodeAlreadyExistsAsync(string taxCode, int companyId, int? id = null)
        {
            var sql = @"SELECT COUNT(1) FROM Finance.TaxCodeMaster
                        WHERE TaxCode = @TaxCode AND CompanyId = @CompanyId AND IsDeleted = 0";
            if (id.HasValue && id.Value > 0)
                sql += " AND Id != @Id";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { TaxCode = taxCode.Trim(), CompanyId = companyId, Id = id });
            return count > 0;
        }

        public async Task<bool> TaxCodeNotFoundAsync(int id)
        {
            const string sql = "SELECT COUNT(1) FROM Finance.TaxCodeMaster WHERE Id = @Id AND IsDeleted = 0";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<bool> TaxCodeExistsAsync(int id)
        {
            const string sql = "SELECT COUNT(1) FROM Finance.TaxCodeMaster WHERE Id = @Id AND IsDeleted = 0";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count > 0;
        }

        public async Task<bool> TaxCodeLinkedAsync(int id)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Finance.TaxAccountLinkage WHERE TaxCodeId = @Id AND IsDeleted = 0
                ) THEN 1 ELSE 0 END";
            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = id });
        }

        // ─── Tax Account Linkage ───────────────────────────────────────────
        private const string LinkageColumns = @"
            tal.Id, tal.CompanyId, tal.TaxCodeId, tc.TaxCode, tc.TaxName,
            tal.GlAccountId, gl.AccountCode, gl.AccountName,
            tal.IsActivated, tal.ApprovalStatus, tal.EffectiveFrom, tal.EffectiveTo,
            tal.IsActive, tal.IsDeleted,
            tal.CreatedBy, tal.CreatedDate, tal.CreatedByName, tal.CreatedIP,
            tal.ModifiedBy, tal.ModifiedDate, tal.ModifiedByName, tal.ModifiedIP";

        private const string LinkageFromJoins = @"
            FROM Finance.TaxAccountLinkage tal
            LEFT JOIN Finance.TaxCodeMaster tc ON tal.TaxCodeId = tc.Id AND tc.IsDeleted = 0
            LEFT JOIN Finance.GlAccountMaster gl ON tal.GlAccountId = gl.Id AND gl.IsDeleted = 0";

        public async Task<(List<TaxAccountLinkageDto>, int)> GetAllLinkagesAsync(int pageNumber, int pageSize, string? searchTerm, int? companyId)
        {
            var whereClause = "tal.IsDeleted = 0";
            if (!string.IsNullOrWhiteSpace(searchTerm))
                whereClause += " AND (tc.TaxCode LIKE @Search OR gl.AccountCode LIKE @Search OR gl.AccountName LIKE @Search)";
            if (companyId.HasValue && companyId.Value > 0)
                whereClause += " AND tal.CompanyId = @CompanyId";

            var query = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*) {LinkageFromJoins} WHERE {whereClause};

                SELECT {LinkageColumns}
                {LinkageFromJoins}
                WHERE {whereClause}
                ORDER BY tal.Id DESC
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

            var result = await _dbConnection.QueryMultipleAsync(query, parameters);
            var list = (await result.ReadAsync<TaxAccountLinkageDto>()).ToList();
            var totalCount = await result.ReadFirstAsync<int>();
            return (list, totalCount);
        }

        public async Task<TaxAccountLinkageDto?> GetLinkageByIdAsync(int id)
        {
            var sql = $@"SELECT {LinkageColumns} {LinkageFromJoins} WHERE tal.Id = @Id AND tal.IsDeleted = 0";
            return await _dbConnection.QueryFirstOrDefaultAsync<TaxAccountLinkageDto>(sql, new { Id = id });
        }

        public async Task<TaxAccountLinkageDto?> GetLinkageByAccountAsync(int glAccountId)
        {
            var sql = $@"SELECT {LinkageColumns} {LinkageFromJoins}
                         WHERE tal.GlAccountId = @GlAccountId AND tal.IsDeleted = 0
                            AND tal.EffectiveTo IS NULL AND tal.ApprovalStatus = 'APPROVED'";
            return await _dbConnection.QueryFirstOrDefaultAsync<TaxAccountLinkageDto>(sql, new { GlAccountId = glAccountId });
        }

        public async Task<List<TaxAccountLinkageDto>> GetLinkageChangeAuditAsync(string? status, int? companyId)
        {
            var whereClause = "tal.IsDeleted = 0";
            if (!string.IsNullOrWhiteSpace(status))
                whereClause += " AND tal.ApprovalStatus = @Status";
            if (companyId.HasValue && companyId.Value > 0)
                whereClause += " AND tal.CompanyId = @CompanyId";

            var sql = $@"SELECT {LinkageColumns} {LinkageFromJoins}
                         WHERE {whereClause}
                         ORDER BY tal.GlAccountId ASC, tal.EffectiveFrom ASC";

            var result = await _dbConnection.QueryAsync<TaxAccountLinkageDto>(sql, new { Status = status, CompanyId = companyId });
            return result.ToList();
        }

        public async Task<bool> GlAccountExistsAsync(int glAccountId)
        {
            const string sql = "SELECT COUNT(1) FROM Finance.GlAccountMaster WHERE Id = @Id AND IsDeleted = 0";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = glAccountId });
            return count > 0;
        }

        public async Task<bool> LinkageNotFoundAsync(int id)
        {
            const string sql = "SELECT COUNT(1) FROM Finance.TaxAccountLinkage WHERE Id = @Id AND IsDeleted = 0";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<bool> LinkageHasGlMappingAsync(int id)
        {
            const string sql = "SELECT COUNT(1) FROM Finance.TaxAccountLinkage WHERE Id = @Id AND GlAccountId > 0 AND IsDeleted = 0";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count > 0;
        }

        // ─── GSTR Section Mapping ──────────────────────────────────────────
        public async Task<(List<GstrSectionMappingDto>, int)> GetAllGstrMappingsAsync(int pageNumber, int pageSize, string? searchTerm, int? companyId)
        {
            var whereClause = "gsm.IsDeleted = 0";
            if (!string.IsNullOrWhiteSpace(searchTerm))
                whereClause += " AND (gsm.GstrType LIKE @Search OR gsm.SectionCode LIKE @Search OR gsm.SectionName LIKE @Search)";
            if (companyId.HasValue && companyId.Value > 0)
                whereClause += " AND gsm.CompanyId = @CompanyId";

            var query = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*) FROM Finance.GstrSectionMapping gsm WHERE {whereClause};

                SELECT gsm.Id, gsm.CompanyId, gsm.GstrType, gsm.SectionCode, gsm.SectionName,
                    gsm.AccountRangeFrom, gsm.AccountRangeTo, gsm.TolerancePercent,
                    gsm.IsActive, gsm.IsDeleted,
                    gsm.CreatedBy, gsm.CreatedDate, gsm.CreatedByName, gsm.CreatedIP,
                    gsm.ModifiedBy, gsm.ModifiedDate, gsm.ModifiedByName, gsm.ModifiedIP
                FROM Finance.GstrSectionMapping gsm
                WHERE {whereClause}
                ORDER BY gsm.CompanyId ASC, gsm.GstrType ASC, gsm.SectionCode ASC
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

            var result = await _dbConnection.QueryMultipleAsync(query, parameters);
            var list = (await result.ReadAsync<GstrSectionMappingDto>()).ToList();
            var totalCount = await result.ReadFirstAsync<int>();
            return (list, totalCount);
        }

        public async Task<GstrSectionMappingDto?> GetGstrMappingByIdAsync(int id)
        {
            const string sql = @"
                SELECT gsm.Id, gsm.CompanyId, gsm.GstrType, gsm.SectionCode, gsm.SectionName,
                    gsm.AccountRangeFrom, gsm.AccountRangeTo, gsm.TolerancePercent,
                    gsm.IsActive, gsm.IsDeleted,
                    gsm.CreatedBy, gsm.CreatedDate, gsm.CreatedByName, gsm.CreatedIP,
                    gsm.ModifiedBy, gsm.ModifiedDate, gsm.ModifiedByName, gsm.ModifiedIP
                FROM Finance.GstrSectionMapping gsm
                WHERE gsm.Id = @Id AND gsm.IsDeleted = 0";

            return await _dbConnection.QueryFirstOrDefaultAsync<GstrSectionMappingDto>(sql, new { Id = id });
        }

        public async Task<bool> GstrMappingAlreadyExistsAsync(int companyId, string gstrType, string sectionCode, int? id = null)
        {
            var sql = @"SELECT COUNT(1) FROM Finance.GstrSectionMapping
                        WHERE CompanyId = @CompanyId AND GstrType = @GstrType AND SectionCode = @SectionCode AND IsDeleted = 0";
            if (id.HasValue && id.Value > 0)
                sql += " AND Id != @Id";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { CompanyId = companyId, GstrType = gstrType.Trim(), SectionCode = sectionCode.Trim(), Id = id });
            return count > 0;
        }

        public async Task<bool> GstrMappingNotFoundAsync(int id)
        {
            const string sql = "SELECT COUNT(1) FROM Finance.GstrSectionMapping WHERE Id = @Id AND IsDeleted = 0";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }
    }
}
