using System.Data;
using Contracts.Dtos.Lookups.Finance;
using Contracts.Interfaces.Lookups.Finance;
using Dapper;

namespace FinanceManagement.Infrastructure.Repositories.Lookups.Finance
{
    internal sealed class TaxCodeLookupRepository : ITaxCodeLookup
    {
        private readonly IDbConnection _dbConnection;

        public TaxCodeLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<IReadOnlyList<TaxCodeLookupDto>> GetAllForCompanyAsync(int companyId, CancellationToken ct = default)
        {
            const string sql = @"
                SELECT tcm.Id, tcm.CompanyId, tcm.TaxCode, tcm.TaxName, tt.Code AS TaxType, tcomp.Code AS TaxComponent,
                    dir.Code AS Direction, tcm.IsSystemOnlyPosting, tcm.StatutorySection,
                    v.RatePercent AS RatePercent
                FROM Finance.TaxCodeMaster tcm
                LEFT JOIN Finance.MiscMaster tt ON tcm.TaxTypeId = tt.Id
                LEFT JOIN Finance.MiscMaster tcomp ON tcm.TaxComponentId = tcomp.Id
                LEFT JOIN Finance.MiscMaster dir ON tcm.DirectionId = dir.Id
                OUTER APPLY (
                    SELECT TOP 1 v2.RatePercent
                    FROM Finance.TaxCodeRateVersion v2
                    WHERE v2.TaxCodeId = tcm.Id AND v2.EffectiveTo IS NULL
                    ORDER BY v2.VersionNo DESC) v
                WHERE tcm.CompanyId = @CompanyId AND tcm.IsActive = 1
                ORDER BY tt.Code ASC, tcm.TaxCode ASC";

            var result = await _dbConnection.QueryAsync<TaxCodeLookupDto>(
                new CommandDefinition(sql, new { CompanyId = companyId }, cancellationToken: ct));
            return result.ToList();
        }

        public async Task<TaxCodeLookupDto?> GetByCodeEffectiveAsync(int companyId, string taxCode, DateOnly asOf, CancellationToken ct = default)
        {
            const string sql = @"
                SELECT tcm.Id, tcm.CompanyId, tcm.TaxCode, tcm.TaxName, tt.Code AS TaxType, tcomp.Code AS TaxComponent,
                    dir.Code AS Direction, tcm.IsSystemOnlyPosting, tcm.StatutorySection,
                    v.RatePercent AS RatePercent
                FROM Finance.TaxCodeMaster tcm
                LEFT JOIN Finance.MiscMaster tt ON tcm.TaxTypeId = tt.Id
                LEFT JOIN Finance.MiscMaster tcomp ON tcm.TaxComponentId = tcomp.Id
                LEFT JOIN Finance.MiscMaster dir ON tcm.DirectionId = dir.Id
                OUTER APPLY (
                    SELECT TOP 1 v2.RatePercent
                    FROM Finance.TaxCodeRateVersion v2
                    WHERE v2.TaxCodeId = tcm.Id
                        AND v2.EffectiveFrom <= @AsOf
                        AND (v2.EffectiveTo IS NULL OR v2.EffectiveTo >= @AsOf)
                    ORDER BY v2.EffectiveFrom DESC) v
                WHERE tcm.TaxCode = @TaxCode AND tcm.CompanyId = @CompanyId
                    AND tcm.IsActive = 1";

            return await _dbConnection.QueryFirstOrDefaultAsync<TaxCodeLookupDto>(
                new CommandDefinition(sql, new { CompanyId = companyId, TaxCode = taxCode, AsOf = asOf }, cancellationToken: ct));
        }
    }
}
