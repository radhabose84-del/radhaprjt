using System.Data;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using Dapper;
using PurchaseManagement.Application.BarcodeAllocation.Dto;
using PurchaseManagement.Application.Common.Interfaces.IBarcodeAllocation;

namespace PurchaseManagement.Infrastructure.Repositories.BarcodeAllocation
{
    public class BarcodeAllocationQueryRepository : IBarcodeAllocationQueryRepository
    {
        // Fixed document-type token for bale barcode allocation (mirrors the command repository).
        private const string AllocationPrefixToken = "BBA";

        private readonly IDbConnection _dbConnection;
        private readonly IFinancialYearLookup _financialYearLookup;
        private readonly IIPAddressService _ipAddressService;

        public BarcodeAllocationQueryRepository(
            IDbConnection dbConnection,
            IFinancialYearLookup financialYearLookup,
            IIPAddressService ipAddressService)
        {
            _dbConnection = dbConnection;
            _financialYearLookup = financialYearLookup;
            _ipAddressService = ipAddressService;
        }

        private const string SelectColumns = @"
            a.Id, a.AllocationNumber, a.AllocationDate, a.EmployeeNo, a.EmployeeName,
            a.BarcodeSeriesId, bs.BarcodeSeriesNumber, p.Code AS Prefix,
            a.BarcodeFrom, a.BarcodeTo,
            (a.BarcodeTo - a.BarcodeFrom + 1) AS TotalAllocatedQuantity,
            a.UsedQuantity,
            (a.BarcodeTo - a.BarcodeFrom + 1 - a.UsedQuantity) AS BalanceQuantity,
            a.StatusId, s.Description AS Status, a.Remarks, a.IsActive
        FROM Purchase.BarcodeAllocation a
        LEFT JOIN Purchase.BarcodeSeries bs ON bs.Id = a.BarcodeSeriesId AND bs.IsDeleted = 0
        LEFT JOIN Purchase.MiscMaster p ON p.Id = bs.PrefixId AND p.IsDeleted = 0
        LEFT JOIN Purchase.MiscMaster s ON s.Id = a.StatusId AND s.IsDeleted = 0";

        public async Task<(List<BarcodeAllocationDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var searchFilter = string.IsNullOrEmpty(searchTerm)
                ? ""
                : "AND (a.AllocationNumber LIKE @Search OR a.EmployeeName LIKE @Search)";

            var query = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Purchase.BarcodeAllocation a
                WHERE a.IsDeleted = 0 {searchFilter};

                SELECT {SelectColumns}
                WHERE a.IsDeleted = 0 {searchFilter}
                ORDER BY a.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;";

            var parameters = new
            {
                Search = $"%{searchTerm}%",
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            using var multi = await _dbConnection.QueryMultipleAsync(query, parameters);
            var list = (await multi.ReadAsync<BarcodeAllocationDto>()).ToList();
            var totalCount = await multi.ReadFirstAsync<int>();

            return (list, totalCount);
        }

        public async Task<BarcodeAllocationDto?> GetByIdAsync(int id)
        {
            var query = $@"SELECT {SelectColumns} WHERE a.Id = @Id AND a.IsDeleted = 0;";
            return await _dbConnection.QueryFirstOrDefaultAsync<BarcodeAllocationDto>(query, new { Id = id });
        }

        public async Task<IReadOnlyList<BarcodeAllocationLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            const string query = @"
                SELECT TOP 20 a.Id, a.AllocationNumber, a.EmployeeName
                FROM Purchase.BarcodeAllocation a
                WHERE a.IsActive = 1 AND a.IsDeleted = 0
                    AND a.AllocationNumber LIKE @Search
                ORDER BY a.Id DESC;";

            var result = await _dbConnection.QueryAsync<BarcodeAllocationLookupDto>(
                new CommandDefinition(query, new { Search = $"%{term}%" }, cancellationToken: ct));
            return result.ToList();
        }

        public async Task<IReadOnlyList<BarcodeAllocationEmployeeDto>> GetEmployeesAsync(string? term, CancellationToken ct)
        {
            // External HR personnel for the current user's division (snapshot source for the passing person).
            var divCode = _ipAddressService.GetOldUnitId();

            var keys = new DataTable();
            keys.Columns.Add("DivCode", typeof(string));
            keys.Columns.Add("EmpNo", typeof(string));
            keys.Rows.Add(divCode, DBNull.Value);

            var parameters = new DynamicParameters();
            parameters.Add("@EmployeeKeys", keys.AsTableValuedParameter("dbo.EmployeeKeyType"));

            var rows = await _dbConnection.QueryAsync<EmployeeRow>(
                new CommandDefinition("dbo.GetEmployeeByDivision_TVP", parameters,
                    commandType: CommandType.StoredProcedure, commandTimeout: 120, cancellationToken: ct));

            var employees = rows.Select(r => new BarcodeAllocationEmployeeDto
            {
                EmployeeNo = r.Empcode.ToString(),
                EmployeeName = r.Empname
            });

            if (!string.IsNullOrWhiteSpace(term))
            {
                var t = term.Trim();
                employees = employees.Where(e =>
                    (e.EmployeeName ?? string.Empty).Contains(t, StringComparison.OrdinalIgnoreCase) ||
                    (e.EmployeeNo ?? string.Empty).Contains(t, StringComparison.OrdinalIgnoreCase));
            }

            return employees.ToList();
        }

        public async Task<IReadOnlyList<BarcodeAllocationSeriesDto>> GetAvailableSeriesAsync(string? term, int? seriesId = null)
        {
            // By id (Edit mode): return just that series — even if fully allocated / inactive.
            // Otherwise: the dropdown list of active series that still have un-allocated range.
            string filter;
            if (seriesId is not null)
            {
                filter = "AND bs.Id = @SeriesId";
            }
            else
            {
                filter = "AND bs.IsActive = 1 AND bs.AllocatedCount < (bs.BarcodeEndNumber - bs.BarcodeStartNumber + 1)";
                if (!string.IsNullOrEmpty(term))
                    filter += " AND bs.BarcodeSeriesNumber LIKE @Search";
            }

            var query = $@"
                SELECT bs.Id, bs.BarcodeSeriesNumber, bs.BarcodeStartNumber, bs.BarcodeEndNumber,
                       (bs.BarcodeEndNumber - bs.BarcodeStartNumber + 1) AS TotalBarcodeCount,
                       bs.AllocatedCount,
                       (bs.BarcodeEndNumber - bs.BarcodeStartNumber + 1 - bs.AllocatedCount) AS BalanceCount,
                       (COALESCE((SELECT MAX(a.BarcodeTo) FROM Purchase.BarcodeAllocation a
                                  WHERE a.BarcodeSeriesId = bs.Id AND a.IsDeleted = 0),
                                 bs.BarcodeStartNumber - 1) + 1) AS NextFrom
                FROM Purchase.BarcodeSeries bs
                WHERE bs.IsDeleted = 0 {filter}
                ORDER BY bs.Id DESC;";

            var result = await _dbConnection.QueryAsync<BarcodeAllocationSeriesDto>(
                query, new { Search = $"%{term}%", SeriesId = seriesId });
            return result.ToList();
        }

        public async Task<bool> RangeOverlapsInSeriesAsync(int barcodeSeriesId, long from, long to, int? id = null)
        {
            var query = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Purchase.BarcodeAllocation
                    WHERE IsDeleted = 0
                        AND BarcodeSeriesId = @SeriesId
                        AND @From <= BarcodeTo
                        AND @To >= BarcodeFrom";

            var parameters = new DynamicParameters(new { SeriesId = barcodeSeriesId, From = from, To = to });

            if (id is not null)
            {
                query += " AND Id <> @Id";
                parameters.Add("Id", id);
            }

            query += ") THEN 1 ELSE 0 END;";

            return await _dbConnection.ExecuteScalarAsync<bool>(query, parameters);
        }

        public async Task<bool> IsWithinSeriesRangeAsync(int barcodeSeriesId, long from, long to)
        {
            const string query = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Purchase.BarcodeSeries
                    WHERE Id = @SeriesId AND IsDeleted = 0
                        AND @From >= BarcodeStartNumber AND @To <= BarcodeEndNumber
                ) THEN 1 ELSE 0 END;";

            return await _dbConnection.ExecuteScalarAsync<bool>(
                query, new { SeriesId = barcodeSeriesId, From = from, To = to });
        }

        public async Task<bool> SeriesExistsAsync(int barcodeSeriesId)
        {
            const string query = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Purchase.BarcodeSeries
                    WHERE Id = @SeriesId AND IsActive = 1 AND IsDeleted = 0
                ) THEN 1 ELSE 0 END;";

            return await _dbConnection.ExecuteScalarAsync<bool>(query, new { SeriesId = barcodeSeriesId });
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string query = "SELECT COUNT(1) FROM Purchase.BarcodeAllocation WHERE Id = @Id AND IsDeleted = 0;";
            var count = await _dbConnection.ExecuteScalarAsync<int>(query, new { Id = id });
            return count > 0;
        }

        public async Task<bool> IsUsedAsync(int id)
        {
            const string query = "SELECT COUNT(1) FROM Purchase.BarcodeAllocation WHERE Id = @Id AND IsDeleted = 0 AND UsedQuantity > 0;";
            var count = await _dbConnection.ExecuteScalarAsync<int>(query, new { Id = id });
            return count > 0;
        }

        public async Task<long?> GetMaxAllocatedToForSeriesAsync(int barcodeSeriesId)
        {
            // Highest already-allocated To, or (series start - 1) when nothing is allocated yet.
            const string query = @"
                SELECT COALESCE(MAX(a.BarcodeTo), bs.BarcodeStartNumber - 1)
                FROM Purchase.BarcodeSeries bs
                LEFT JOIN Purchase.BarcodeAllocation a
                    ON a.BarcodeSeriesId = bs.Id AND a.IsDeleted = 0
                WHERE bs.Id = @SeriesId AND bs.IsDeleted = 0
                GROUP BY bs.BarcodeStartNumber;";

            return await _dbConnection.ExecuteScalarAsync<long?>(query, new { SeriesId = barcodeSeriesId });
        }

        public async Task<string> PeekNextAllocationNumberAsync(DateTimeOffset allocationDate)
        {
            var allocationDay = allocationDate.Date;

            var financialYears = await _financialYearLookup.GetAllFinancialYearAsync();
            var financialYear = financialYears
                .FirstOrDefault(f => f.StartDate.Date <= allocationDay && allocationDay <= f.EndDate.Date);

            if (financialYear == null || string.IsNullOrWhiteSpace(financialYear.StartYear))
                return string.Empty;

            var prefix = $"{AllocationPrefixToken}-{financialYear.StartYear}-";

            const string query = @"
                SELECT TOP 1 AllocationNumber
                FROM Purchase.BarcodeAllocation
                WHERE AllocationNumber LIKE @Prefix
                ORDER BY AllocationNumber DESC;";

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

        private sealed class EmployeeRow
        {
            public int Empcode { get; set; }
            public string? Empname { get; set; }
        }
    }
}
