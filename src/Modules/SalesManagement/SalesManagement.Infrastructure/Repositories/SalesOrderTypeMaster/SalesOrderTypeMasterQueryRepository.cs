using System.Data;
using Contracts.Interfaces.Lookups.Finance;
using Contracts.Interfaces.Lookups.Users;
using Dapper;
using SalesManagement.Application.Common.Constants;
using SalesManagement.Application.Common.Interfaces.ISalesOrderTypeMaster;
using SalesManagement.Application.SalesOrderTypeMaster.Dto;

namespace SalesManagement.Infrastructure.Repositories.SalesOrderTypeMaster
{
    public class SalesOrderTypeMasterQueryRepository : ISalesOrderTypeMasterQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly ITransactionTypeLookup _transactionTypeLookup;
        private readonly ICurrencyLookup _currencyLookup;

        public SalesOrderTypeMasterQueryRepository(
            IDbConnection dbConnection,
            ITransactionTypeLookup transactionTypeLookup,
            ICurrencyLookup currencyLookup)
        {
            _dbConnection = dbConnection;
            _transactionTypeLookup = transactionTypeLookup;
            _currencyLookup = currencyLookup;
        }

        // ============================================================
        //  GetAllAsync — paginated + searchable, with same-module JOIN
        // ============================================================
        public async Task<(List<SalesOrderTypeMasterDto>, int)> GetAllAsync(
            int pageNumber, int pageSize, string? searchTerm)
        {
            var offset = (pageNumber - 1) * pageSize;
            var hasSearch = !string.IsNullOrWhiteSpace(searchTerm);

            var whereClause = "WHERE s.IsDeleted = 0";
            if (hasSearch)
            {
                whereClause += " AND (s.TypeName LIKE @Search OR mm.Code LIKE @Search OR mm.Description LIKE @Search)";
            }

            var sql = $@"
                SELECT s.Id, s.SoTypeId, s.TaxTypeId, s.TypeName, s.Description,
                       s.AllowsDispatch, s.RequiresValidity, s.AllowZeroPrice,
                       s.MinPrice, s.MaxPrice, s.MaxQty,
                       s.AllowPriceOverride, s.OverrideLimitPercent, s.ApprovalRequired,
                       s.CurrencyRequired, s.AllowIGST, s.CountryMandatory, s.DefaultCurrencyId,
                       s.IsActive, s.IsDeleted,
                       s.CreatedBy, s.CreatedDate, s.CreatedByName, s.CreatedIP,
                       s.ModifiedBy, s.ModifiedDate, s.ModifiedByName, s.ModifiedIP,
                       mm.Code        AS SoTypeCode,
                       mm.Description AS SoTypeName
                FROM   Sales.SalesOrderTypeMaster s
                LEFT JOIN Sales.MiscMaster mm
                    ON  s.SoTypeId = mm.Id
                    AND mm.IsDeleted = 0
                {whereClause}
                ORDER BY s.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT COUNT(*)
                FROM   Sales.SalesOrderTypeMaster s
                LEFT JOIN Sales.MiscMaster mm
                    ON  s.SoTypeId = mm.Id
                    AND mm.IsDeleted = 0
                {whereClause};
            ";

            using var multi = await _dbConnection.QueryMultipleAsync(sql, new
            {
                Search = $"%{searchTerm}%",
                Offset = offset,
                PageSize = pageSize
            });

            var data = (await multi.ReadAsync<SalesOrderTypeMasterDto>()).ToList();
            var totalCount = await multi.ReadFirstAsync<int>();

            await EnrichWithLookupsAsync(data);

            return (data, totalCount);
        }

        // ============================================================
        //  GetByIdAsync
        // ============================================================
        public async Task<SalesOrderTypeMasterDto?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT s.Id, s.SoTypeId, s.TaxTypeId, s.TypeName, s.Description,
                       s.AllowsDispatch, s.RequiresValidity, s.AllowZeroPrice,
                       s.MinPrice, s.MaxPrice, s.MaxQty,
                       s.AllowPriceOverride, s.OverrideLimitPercent, s.ApprovalRequired,
                       s.CurrencyRequired, s.AllowIGST, s.CountryMandatory, s.DefaultCurrencyId,
                       s.IsActive, s.IsDeleted,
                       s.CreatedBy, s.CreatedDate, s.CreatedByName, s.CreatedIP,
                       s.ModifiedBy, s.ModifiedDate, s.ModifiedByName, s.ModifiedIP,
                       mm.Code        AS SoTypeCode,
                       mm.Description AS SoTypeName
                FROM   Sales.SalesOrderTypeMaster s
                LEFT JOIN Sales.MiscMaster mm
                    ON  s.SoTypeId = mm.Id
                    AND mm.IsDeleted = 0
                WHERE  s.Id = @Id AND s.IsDeleted = 0";

            var dto = await _dbConnection.QueryFirstOrDefaultAsync<SalesOrderTypeMasterDto>(sql, new { Id = id });

            if (dto != null)
            {
                await EnrichWithLookupsAsync(new List<SalesOrderTypeMasterDto> { dto });
            }

            return dto;
        }

        // ============================================================
        //  AutocompleteAsync — Active + NotDeleted only
        // ============================================================
        public async Task<IReadOnlyList<SalesOrderTypeMasterLookupDto>> AutocompleteAsync(
            string term, CancellationToken cancellationToken)
        {
            var hasTerm = !string.IsNullOrWhiteSpace(term);

            var sql = $@"
                SELECT s.Id, s.SoTypeId, s.TaxTypeId, s.TypeName,
                       mm.Code AS SoTypeCode
                FROM   Sales.SalesOrderTypeMaster s
                LEFT JOIN Sales.MiscMaster mm
                    ON  s.SoTypeId = mm.Id
                    AND mm.IsDeleted = 0
                WHERE  s.IsActive = 1 AND s.IsDeleted = 0
                {(hasTerm ? "AND (s.TypeName LIKE @Term OR mm.Code LIKE @Term)" : string.Empty)}
                ORDER BY s.TypeName";

            var rows = (await _dbConnection.QueryAsync<SalesOrderTypeMasterLookupDto>(
                sql, new { Term = $"%{term}%" })).ToList();

            // Enrich TaxTypeShortName via Finance lookup
            var taxIds = rows.Select(r => r.TaxTypeId).Where(i => i > 0).Distinct().ToList();
            if (taxIds.Count > 0)
            {
                var txTypes = await _transactionTypeLookup.GetByIdsAsync(taxIds);
                var txDict = txTypes.ToDictionary(t => t.Id, t => t.ShortName);

                foreach (var r in rows)
                {
                    if (txDict.TryGetValue(r.TaxTypeId, out var sn))
                        r.TaxTypeShortName = sn;
                }
            }

            return rows;
        }

        // ============================================================
        //  Validation helpers
        // ============================================================
        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"SELECT COUNT(1)
                                 FROM Sales.SalesOrderTypeMaster
                                 WHERE Id = @Id AND IsDeleted = 0";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<bool> AlreadyExistsAsync(int soTypeId, int taxTypeId, int? excludeId = null)
        {
            var sql = @"SELECT COUNT(1)
                        FROM Sales.SalesOrderTypeMaster
                        WHERE SoTypeId = @SoTypeId
                          AND TaxTypeId = @TaxTypeId
                          AND IsDeleted = 0";

            if (excludeId.HasValue && excludeId.Value > 0)
                sql += " AND Id != @ExcludeId";

            var count = await _dbConnection.ExecuteScalarAsync<int>(
                sql, new { SoTypeId = soTypeId, TaxTypeId = taxTypeId, ExcludeId = excludeId });

            return count > 0;
        }

        public async Task<bool> IsValidSoTypeAsync(int soTypeId)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1
                    FROM   Sales.MiscMaster mm
                    JOIN   Sales.MiscTypeMaster mt ON mm.MiscTypeId = mt.Id
                    WHERE  mm.Id = @SoTypeId
                      AND  mm.IsActive = 1 AND mm.IsDeleted = 0
                      AND  mt.MiscTypeCode = @MiscTypeCode
                      AND  mt.IsActive = 1 AND mt.IsDeleted = 0
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(
                sql, new { SoTypeId = soTypeId, MiscTypeCode = MiscMasterCodes.SOTM_TYPE_MISCTYPE });
        }

        public async Task<string?> GetSoTypeCodeAsync(int soTypeId)
        {
            const string sql = @"
                SELECT mm.Code
                FROM   Sales.MiscMaster mm
                JOIN   Sales.MiscTypeMaster mt ON mm.MiscTypeId = mt.Id
                WHERE  mm.Id = @SoTypeId
                  AND  mm.IsDeleted = 0
                  AND  mt.MiscTypeCode = @MiscTypeCode
                  AND  mt.IsDeleted = 0";

            return await _dbConnection.QueryFirstOrDefaultAsync<string>(
                sql, new { SoTypeId = soTypeId, MiscTypeCode = MiscMasterCodes.SOTM_TYPE_MISCTYPE });
        }

        public async Task<int?> GetSoTypeIdByRowIdAsync(int id)
        {
            const string sql = @"
                SELECT SoTypeId
                FROM   Sales.SalesOrderTypeMaster
                WHERE  Id = @Id AND IsDeleted = 0";

            return await _dbConnection.QueryFirstOrDefaultAsync<int?>(sql, new { Id = id });
        }

        // ============================================================
        //  Helper — populate cross-module lookups (TaxType, Currency)
        // ============================================================
        private async Task EnrichWithLookupsAsync(List<SalesOrderTypeMasterDto> data)
        {
            if (data.Count == 0) return;

            // TaxType (Finance.TransactionTypeMaster)
            var taxIds = data.Select(d => d.TaxTypeId).Where(i => i > 0).Distinct().ToList();
            if (taxIds.Count > 0)
            {
                var txTypes = await _transactionTypeLookup.GetByIdsAsync(taxIds);
                var txDict = txTypes.ToDictionary(t => t.Id, t => t);

                foreach (var item in data)
                {
                    if (txDict.TryGetValue(item.TaxTypeId, out var tx))
                    {
                        item.TaxTypeName      = tx.TypeName;
                        item.TaxTypeShortName = tx.ShortName;
                    }
                }
            }

            // Currency (UserManagement.AppData.Currency)
            var currencyIds = data
                .Where(d => d.DefaultCurrencyId.HasValue)
                .Select(d => d.DefaultCurrencyId!.Value)
                .Distinct()
                .ToList();

            if (currencyIds.Count > 0)
            {
                var currencies = await _currencyLookup.GetByIdsAsync(currencyIds);
                var ccyDict = currencies.ToDictionary(c => c.CurrencyId, c => c);

                foreach (var item in data.Where(d => d.DefaultCurrencyId.HasValue))
                {
                    if (ccyDict.TryGetValue(item.DefaultCurrencyId!.Value, out var ccy))
                    {
                        item.DefaultCurrencyCode = ccy.Code;
                    }
                }
            }
        }
    }
}
