using System.Data;
using Contracts.Interfaces.Lookups.Users;
using Dapper;
using PurchaseManagement.Application.BarcodeSeries.Dto;
using PurchaseManagement.Application.Common.Interfaces.IBarcodeSeries;
using PurchaseManagement.Domain.Common;

namespace PurchaseManagement.Infrastructure.Repositories.BarcodeSeries
{
    public class BarcodeSeriesQueryRepository : IBarcodeSeriesQueryRepository
    {
        // Fixed document-type token for bale barcode series (mirrors the command repository).
        private const string SeriesPrefixToken = "BCS";

        private readonly IDbConnection _dbConnection;
        private readonly IFinancialYearLookup _financialYearLookup;

        public BarcodeSeriesQueryRepository(IDbConnection dbConnection, IFinancialYearLookup financialYearLookup)
        {
            _dbConnection = dbConnection;
            _financialYearLookup = financialYearLookup;
        }

        public async Task<(List<BarcodeSeriesDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var searchFilter = string.IsNullOrEmpty(searchTerm) ? "" : "AND b.BarcodeSeriesNumber LIKE @Search";

            var query = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Purchase.BarcodeSeries b
                WHERE b.IsDeleted = 0 {searchFilter};

                SELECT b.Id, b.BarcodeSeriesNumber, b.PrefixId, p.Code AS Prefix, b.GenerationDate,
                       b.BarcodeStartNumber, b.BarcodeEndNumber,
                       (b.BarcodeEndNumber - b.BarcodeStartNumber + 1) AS TotalBarcodeCount, b.AllocatedCount,
                       (b.BarcodeEndNumber - b.BarcodeStartNumber + 1 - b.AllocatedCount) AS Balance,
                       b.StatusId, s.Description AS Status, b.Remarks, b.IsActive
                FROM Purchase.BarcodeSeries b
                LEFT JOIN Purchase.MiscMaster p ON p.Id = b.PrefixId AND p.IsDeleted = 0
                LEFT JOIN Purchase.MiscMaster s ON s.Id = b.StatusId AND s.IsDeleted = 0
                WHERE b.IsDeleted = 0 {searchFilter}
                ORDER BY b.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;";

            var parameters = new
            {
                Search = $"%{searchTerm}%",
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            using var multi = await _dbConnection.QueryMultipleAsync(query, parameters);
            var list = (await multi.ReadAsync<BarcodeSeriesDto>()).ToList();
            var totalCount = await multi.ReadFirstAsync<int>();

            foreach (var item in list)
                item.BarcodeFormatPreview = $"{item.Prefix}{item.BarcodeStartNumber}";

            return (list, totalCount);
        }

        public async Task<BarcodeSeriesDto?> GetByIdAsync(int id)
        {
            const string query = @"
                SELECT b.Id, b.BarcodeSeriesNumber, b.PrefixId, p.Code AS Prefix, b.GenerationDate,
                       b.BarcodeStartNumber, b.BarcodeEndNumber,
                       (b.BarcodeEndNumber - b.BarcodeStartNumber + 1) AS TotalBarcodeCount, b.AllocatedCount,
                       (b.BarcodeEndNumber - b.BarcodeStartNumber + 1 - b.AllocatedCount) AS Balance,
                       b.StatusId, s.Description AS Status, b.Remarks, b.IsActive
                FROM Purchase.BarcodeSeries b
                LEFT JOIN Purchase.MiscMaster p ON p.Id = b.PrefixId AND p.IsDeleted = 0
                LEFT JOIN Purchase.MiscMaster s ON s.Id = b.StatusId AND s.IsDeleted = 0
                WHERE b.Id = @Id AND b.IsDeleted = 0;";

            var dto = await _dbConnection.QueryFirstOrDefaultAsync<BarcodeSeriesDto>(query, new { Id = id });
            if (dto != null)
                dto.BarcodeFormatPreview = $"{dto.Prefix}{dto.BarcodeStartNumber}";

            return dto;
        }

        public async Task<IReadOnlyList<BarcodeSeriesLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            const string query = @"
                SELECT TOP 20 b.Id, b.BarcodeSeriesNumber, p.Code AS Prefix
                FROM Purchase.BarcodeSeries b
                LEFT JOIN Purchase.MiscMaster p ON p.Id = b.PrefixId AND p.IsDeleted = 0
                WHERE b.IsActive = 1 AND b.IsDeleted = 0
                    AND b.BarcodeSeriesNumber LIKE @Search
                ORDER BY b.Id DESC;";

            var result = await _dbConnection.QueryAsync<BarcodeSeriesLookupDto>(
                new CommandDefinition(query, new { Search = $"%{term}%" }, cancellationToken: ct));
            return result.ToList();
        }

        public async Task<bool> RangeOverlapsAsync(long start, long end, int? id = null)
        {
            var query = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Purchase.BarcodeSeries
                    WHERE IsDeleted = 0
                        AND @StartNumber <= BarcodeEndNumber
                        AND @EndNumber >= BarcodeStartNumber";

            var parameters = new DynamicParameters(new { StartNumber = start, EndNumber = end });

            if (id is not null)
            {
                query += " AND Id <> @Id";
                parameters.Add("Id", id);
            }

            query += ") THEN 1 ELSE 0 END;";

            return await _dbConnection.ExecuteScalarAsync<bool>(query, parameters);
        }

        public async Task<bool> IsValidPrefixAsync(int prefixId)
        {
            const string query = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Purchase.MiscMaster m
                    INNER JOIN Purchase.MiscTypeMaster mt ON mt.Id = m.MiscTypeId
                    WHERE m.Id = @PrefixId AND m.IsActive = 1 AND m.IsDeleted = 0
                        AND mt.IsDeleted = 0 AND mt.MiscTypeCode = @TypeCode
                ) THEN 1 ELSE 0 END;";

            return await _dbConnection.ExecuteScalarAsync<bool>(
                query, new { PrefixId = prefixId, TypeCode = MiscEnumEntity.BarcodePrefix });
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string query = "SELECT COUNT(1) FROM Purchase.BarcodeSeries WHERE Id = @Id AND IsDeleted = 0;";
            var count = await _dbConnection.ExecuteScalarAsync<int>(query, new { Id = id });
            return count > 0;
        }

        public async Task<bool> IsAllocatedAsync(int id)
        {
            // Blocked when the series has any live allocation drawn from it.
            const string query = @"
                SELECT COUNT(1) FROM Purchase.BarcodeAllocation
                WHERE BarcodeSeriesId = @Id AND IsDeleted = 0;";
            var count = await _dbConnection.ExecuteScalarAsync<int>(query, new { Id = id });
            return count > 0;
        }

        public async Task<long?> GetMaxEndNumberAsync()
        {
            const string query = "SELECT MAX(BarcodeEndNumber) FROM Purchase.BarcodeSeries WHERE IsDeleted = 0;";
            return await _dbConnection.ExecuteScalarAsync<long?>(query);
        }

        public async Task<string> PeekNextSeriesNumberAsync(DateTimeOffset generationDate)
        {
            var generationDay = generationDate.Date;

            var financialYears = await _financialYearLookup.GetAllFinancialYearAsync();
            var financialYear = financialYears
                .FirstOrDefault(f => f.StartDate.Date <= generationDay && generationDay <= f.EndDate.Date);

            if (financialYear == null || string.IsNullOrWhiteSpace(financialYear.StartYear))
                return string.Empty;

            var prefix = $"{SeriesPrefixToken}-{financialYear.StartYear}-";

            const string query = @"
                SELECT TOP 1 BarcodeSeriesNumber
                FROM Purchase.BarcodeSeries
                WHERE BarcodeSeriesNumber LIKE @Prefix
                ORDER BY BarcodeSeriesNumber DESC;";

            var lastNumber = await _dbConnection.QueryFirstOrDefaultAsync<string>(
                query, new { Prefix = $"{prefix}%" });

            var nextSerial = 1;
            if (!string.IsNullOrEmpty(lastNumber))
            {
                var lastDash = lastNumber.LastIndexOf('-');
                if (lastDash >= 0 && int.TryParse(lastNumber[(lastDash + 1)..], out var parsed))
                    nextSerial = parsed + 1;
            }

            return $"{prefix}{nextSerial:D4}";
        }
    }
}
