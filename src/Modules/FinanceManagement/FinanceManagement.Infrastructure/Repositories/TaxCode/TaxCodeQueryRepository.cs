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
            tcm.Id, tcm.CompanyId, tcm.TaxCode, tcm.TaxName,
            tcm.TaxTypeId, tt.Code AS TaxType, tcm.TaxComponentId, tcomp.Code AS TaxComponent,
            tcm.ParentTaxCodeId, p.TaxCode AS ParentTaxCode,
            tcm.DirectionId, dir.Code AS Direction,
            tcm.StatutorySection, tcm.ThresholdAmount, tcm.ThresholdAggregate, tcm.HsnSacCode,
            tcm.IsSystemOnlyPosting, tcm.IsEefcRelevant, tcm.IsStatutoryFixed,
            v.RatePercent AS CurrentRatePercent, v.EffectiveFrom AS CurrentEffectiveFrom,
            tcm.IsActive,
            tcm.CreatedBy, tcm.CreatedDate, tcm.CreatedByName, tcm.CreatedIP,
            tcm.ModifiedBy, tcm.ModifiedDate, tcm.ModifiedByName, tcm.ModifiedIP";

        private const string TaxCodeFromJoins = @"
            FROM Finance.TaxCodeMaster tcm
            LEFT JOIN Finance.TaxCodeMaster p ON tcm.ParentTaxCodeId = p.Id
            LEFT JOIN Finance.MiscMaster tt ON tcm.TaxTypeId = tt.Id
            LEFT JOIN Finance.MiscMaster tcomp ON tcm.TaxComponentId = tcomp.Id
            LEFT JOIN Finance.MiscMaster dir ON tcm.DirectionId = dir.Id
            OUTER APPLY (
                SELECT TOP 1 v2.RatePercent, v2.EffectiveFrom
                FROM Finance.TaxCodeRateVersion v2
                WHERE v2.TaxCodeId = tcm.Id AND v2.EffectiveTo IS NULL
                ORDER BY v2.VersionNo DESC) v";

        public async Task<(List<TaxCodeMasterDto>, int)> GetAllTaxCodesAsync(int pageNumber, int pageSize, string? searchTerm, int? companyId, string? taxType)
        {
            var whereClause = "1 = 1";
            if (!string.IsNullOrWhiteSpace(searchTerm))
                whereClause += " AND (tcm.TaxCode LIKE @Search OR tcm.TaxName LIKE @Search)";
            if (companyId.HasValue && companyId.Value > 0)
                whereClause += " AND tcm.CompanyId = @CompanyId";
            if (!string.IsNullOrWhiteSpace(taxType))
                whereClause += " AND tcm.TaxTypeId IN (SELECT mmf.Id FROM Finance.MiscMaster mmf WHERE UPPER(REPLACE(mmf.Code,' ','_')) = UPPER(@TaxType))";

            var query = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*) FROM Finance.TaxCodeMaster tcm WHERE {whereClause};

                SELECT {TaxCodeColumns}
                {TaxCodeFromJoins}
                WHERE {whereClause}
                ORDER BY tcm.CompanyId ASC, tt.Code ASC, tcm.TaxCode ASC
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
            var sql = $@"SELECT {TaxCodeColumns} {TaxCodeFromJoins} WHERE tcm.Id = @Id";
            var dto = await _dbConnection.QueryFirstOrDefaultAsync<TaxCodeMasterDto>(sql, new { Id = id });

            // Merged from the former versions API — return the full rate history inline.
            if (dto != null)
                dto.RateVersions = await GetRateVersionsAsync(id);

            return dto;
        }

        public async Task<IReadOnlyList<TaxCodeMasterLookupDto>> TaxCodeAutocompleteAsync(string term, int? companyId, string? taxType, CancellationToken ct)
        {
            var whereClause = "tcm.IsActive = 1";
            if (!string.IsNullOrWhiteSpace(term))
                whereClause += " AND (tcm.TaxCode LIKE @Term OR tcm.TaxName LIKE @Term)";
            if (companyId.HasValue && companyId.Value > 0)
                whereClause += " AND tcm.CompanyId = @CompanyId";
            if (!string.IsNullOrWhiteSpace(taxType))
                whereClause += " AND tcm.TaxTypeId IN (SELECT mmf.Id FROM Finance.MiscMaster mmf WHERE UPPER(REPLACE(mmf.Code,' ','_')) = UPPER(@TaxType))";

            var sql = $@"
                SELECT tcm.Id, tcm.CompanyId, tcm.TaxCode, tcm.TaxName,
                    tcm.TaxTypeId, tt.Code AS TaxType, tcm.TaxComponentId, tcomp.Code AS TaxComponent,
                    v.RatePercent AS CurrentRatePercent
                FROM Finance.TaxCodeMaster tcm
                LEFT JOIN Finance.MiscMaster tt ON tcm.TaxTypeId = tt.Id
                LEFT JOIN Finance.MiscMaster tcomp ON tcm.TaxComponentId = tcomp.Id
                OUTER APPLY (
                    SELECT TOP 1 v2.RatePercent
                    FROM Finance.TaxCodeRateVersion v2
                    WHERE v2.TaxCodeId = tcm.Id AND v2.EffectiveTo IS NULL
                    ORDER BY v2.VersionNo DESC) v
                WHERE {whereClause}
                ORDER BY tt.Code ASC, tcm.TaxCode ASC";

            var result = await _dbConnection.QueryAsync<TaxCodeMasterLookupDto>(
                new CommandDefinition(sql, new { Term = $"%{term}%", CompanyId = companyId, TaxType = taxType }, cancellationToken: ct));
            return result.ToList();
        }

        public async Task<List<TaxCodeRateVersionDto>> GetRateVersionsAsync(int taxCodeId)
        {
            const string sql = @"
                SELECT Id, TaxCodeId, VersionNo, RatePercent, EffectiveFrom, EffectiveTo, ChangeReason,
                    IsActive, CreatedBy, CreatedDate, CreatedByName
                FROM Finance.TaxCodeRateVersion
                WHERE TaxCodeId = @TaxCodeId
                ORDER BY VersionNo ASC";

            var result = await _dbConnection.QueryAsync<TaxCodeRateVersionDto>(sql, new { TaxCodeId = taxCodeId });
            return result.ToList();
        }

        public async Task<bool> TaxCodeAlreadyExistsAsync(string taxCode, int companyId, int? id = null)
        {
            var sql = @"SELECT COUNT(1) FROM Finance.TaxCodeMaster
                        WHERE TaxCode = @TaxCode AND CompanyId = @CompanyId";
            if (id.HasValue && id.Value > 0)
                sql += " AND Id != @Id";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { TaxCode = taxCode.Trim(), CompanyId = companyId, Id = id });
            return count > 0;
        }

        public async Task<bool> TaxCodeNotFoundAsync(int id)
        {
            const string sql = "SELECT COUNT(1) FROM Finance.TaxCodeMaster WHERE Id = @Id";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<bool> TaxCodeExistsAsync(int id)
        {
            const string sql = "SELECT COUNT(1) FROM Finance.TaxCodeMaster WHERE Id = @Id";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count > 0;
        }

        // Returns the MiscMaster code for an id, normalised (UPPER, spaces->underscores) — used by the
        // validator to apply the GST/IGST/TDS/CUSTOMS business rules from the selected TaxTypeId/etc.
        public async Task<string?> GetMiscCodeAsync(int miscMasterId)
        {
            const string sql = "SELECT UPPER(REPLACE(Code, ' ', '_')) FROM Finance.MiscMaster WHERE Id = @Id AND IsDeleted = 0";
            return await _dbConnection.ExecuteScalarAsync<string?>(sql, new { Id = miscMasterId });
        }

        // Separator-insensitive match: strip BOTH spaces and underscores so seeded
        // MiscTypeCode 'TaxType', 'TAX_TYPE' and 'TAX TYPE' all compare equal to 'TAXTYPE'.
        private async Task<bool> MiscExistsAsync(int id, string miscTypeCode)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Finance.MiscMaster mm
                JOIN Finance.MiscTypeMaster mt ON mm.MiscTypeId = mt.Id AND mt.IsDeleted = 0
                WHERE mm.Id = @Id AND mm.IsActive = 1 AND mm.IsDeleted = 0
                    AND UPPER(REPLACE(REPLACE(mt.MiscTypeCode, ' ', ''), '_', '')) = @TypeCode";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id, TypeCode = miscTypeCode });
            return count > 0;
        }

        public Task<bool> TaxTypeExistsAsync(int id) => MiscExistsAsync(id, "TAXTYPE");
        public Task<bool> TaxComponentExistsAsync(int id) => MiscExistsAsync(id, "TAXCOMPONENT");
        public Task<bool> DirectionExistsAsync(int id) => MiscExistsAsync(id, "TAXDIRECTION");

        // Resolve a MiscMaster value id by misc-type code + value code (both separator-insensitive).
        public async Task<int?> GetMiscIdAsync(string miscTypeCode, string valueCode)
        {
            const string sql = @"
                SELECT TOP 1 mm.Id
                FROM Finance.MiscMaster mm
                JOIN Finance.MiscTypeMaster mt ON mm.MiscTypeId = mt.Id AND mt.IsDeleted = 0
                WHERE mm.IsDeleted = 0
                    AND UPPER(REPLACE(REPLACE(mt.MiscTypeCode, ' ', ''), '_', '')) = UPPER(REPLACE(REPLACE(@TypeCode, ' ', ''), '_', ''))
                    AND UPPER(REPLACE(REPLACE(mm.Code, ' ', ''), '_', '')) = UPPER(REPLACE(REPLACE(@ValueCode, ' ', ''), '_', ''))";
            return await _dbConnection.ExecuteScalarAsync<int?>(sql, new { TypeCode = miscTypeCode, ValueCode = valueCode });
        }

        // ─── Tax Account Linkage ───────────────────────────────────────────
        private const string LinkageColumns = @"
            tal.Id, tal.CompanyId, tal.TaxCodeId, tc.TaxCode, tc.TaxName,
            tal.GlAccountId, gl.AccountCode, gl.AccountName,
            tal.ControlAccountId, cat.Description AS ControlAccountName,
            tal.StatusId, st.Code AS Status, tal.EffectiveFrom, tal.EffectiveTo,
            tal.ChangeReason,
            tal.IsActive,
            tal.CreatedBy, tal.CreatedDate, tal.CreatedByName, tal.CreatedIP,
            tal.ModifiedBy, tal.ModifiedDate, tal.ModifiedByName, tal.ModifiedIP";

        private const string LinkageFromJoins = @"
            FROM Finance.TaxAccountLinkage tal
            LEFT JOIN Finance.TaxCodeMaster tc ON tal.TaxCodeId = tc.Id
            LEFT JOIN Finance.GlAccountMaster gl ON tal.GlAccountId = gl.Id AND gl.IsDeleted = 0
            LEFT JOIN Finance.MiscMaster cat ON tal.ControlAccountId = cat.Id AND cat.IsDeleted = 0
            LEFT JOIN Finance.MiscMaster st ON tal.StatusId = st.Id AND st.IsDeleted = 0";

        public async Task<(List<TaxAccountLinkageDto>, int)> GetAllLinkagesAsync(int pageNumber, int pageSize, string? searchTerm, int? companyId, int? statusId)
        {
            var whereClause = "1 = 1";
            if (!string.IsNullOrWhiteSpace(searchTerm))
                whereClause += " AND (tc.TaxCode LIKE @Search OR gl.AccountCode LIKE @Search OR gl.AccountName LIKE @Search)";
            if (companyId.HasValue && companyId.Value > 0)
                whereClause += " AND tal.CompanyId = @CompanyId";
            if (statusId.HasValue && statusId.Value > 0)
                whereClause += " AND tal.StatusId = @StatusId";

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
                StatusId = statusId,
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
            var sql = $@"SELECT {LinkageColumns} {LinkageFromJoins} WHERE tal.Id = @Id";
            return await _dbConnection.QueryFirstOrDefaultAsync<TaxAccountLinkageDto>(sql, new { Id = id });
        }

        public async Task<TaxAccountLinkageDto?> GetLinkageByAccountAsync(int glAccountId)
        {
            var sql = $@"SELECT {LinkageColumns} {LinkageFromJoins}
                         WHERE tal.GlAccountId = @GlAccountId
                            AND tal.EffectiveTo IS NULL AND tal.IsActive = 1";
            return await _dbConnection.QueryFirstOrDefaultAsync<TaxAccountLinkageDto>(sql, new { GlAccountId = glAccountId });
        }

        public async Task<bool> GlAccountExistsAsync(int glAccountId)
        {
            const string sql = "SELECT COUNT(1) FROM Finance.GlAccountMaster WHERE Id = @Id AND IsDeleted = 0";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = glAccountId });
            return count > 0;
        }

        // Valid only when the MiscMaster row is active and belongs to the CONTROL ACCOUNT TYPE
        // misc-type. Separator-insensitive ('CONTROL ACCOUNT TYPE' / 'CONTROL_ACCOUNT_TYPE' / 'ControlAccountType').
        public async Task<bool> ControlAccountExistsAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Finance.MiscMaster mm
                JOIN Finance.MiscTypeMaster mt ON mm.MiscTypeId = mt.Id AND mt.IsDeleted = 0
                WHERE mm.Id = @Id AND mm.IsActive = 1 AND mm.IsDeleted = 0
                    AND UPPER(REPLACE(REPLACE(mt.MiscTypeCode, ' ', ''), '_', '')) = 'CONTROLACCOUNTTYPE'";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count > 0;
        }

        public async Task<bool> LinkageNotFoundAsync(int id)
        {
            const string sql = "SELECT COUNT(1) FROM Finance.TaxAccountLinkage WHERE Id = @Id";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<bool> LinkageHasGlMappingAsync(int id)
        {
            const string sql = "SELECT COUNT(1) FROM Finance.TaxAccountLinkage WHERE Id = @Id AND GlAccountId > 0";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count > 0;
        }

    }
}
