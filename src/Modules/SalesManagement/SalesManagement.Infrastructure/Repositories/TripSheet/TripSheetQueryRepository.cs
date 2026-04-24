using System.Data;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Users;
using Dapper;
using SalesManagement.Application.Common.Interfaces.ITripSheet;
using SalesManagement.Application.TripSheet.Dto;

namespace SalesManagement.Infrastructure.Repositories.TripSheet
{
    public class TripSheetQueryRepository : ITripSheetQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IUnitLookup _unitLookup;
        private readonly IPartyLookup _partyLookup;

        public TripSheetQueryRepository(
            IDbConnection dbConnection,
            IUnitLookup unitLookup,
            IPartyLookup partyLookup)
        {
            _dbConnection = dbConnection;
            _unitLookup = unitLookup;
            _partyLookup = partyLookup;
        }

        public async Task<(List<TripSheetHeaderDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var offset = (pageNumber - 1) * pageSize;

            const string countSql = @"
                SELECT COUNT(*)
                FROM Sales.TripSheetHeader h
                WHERE h.IsDeleted = 0
                  AND (@SearchTerm IS NULL OR h.TripSheetNo LIKE '%' + @SearchTerm + '%'
                       OR h.VehicleNo LIKE '%' + @SearchTerm + '%')";

            const string dataSql = @"
                SELECT h.Id, h.TripSheetNo, h.TripDate, h.VehicleNo, h.UnitId,
                       h.Remarks, h.IsActive, h.IsDeleted,
                       h.CreatedBy, h.CreatedDate, h.CreatedByName, h.CreatedIP,
                       h.ModifiedBy, h.ModifiedDate, h.ModifiedByName, h.ModifiedIP,
                       (SELECT COUNT(*) FROM Sales.TripSheetDetail d WHERE d.TripSheetHeaderId = h.Id) AS TotalDispatches,
                       (SELECT COUNT(*) FROM Sales.TripSheetDetail d
                        INNER JOIN Sales.DispatchAdviceHeader da ON d.DispatchAdviceHeaderId = da.Id
                        WHERE d.TripSheetHeaderId = h.Id AND da.InvFlg = 1 AND da.IsDeleted = 0) AS TotalInvoiced
                FROM Sales.TripSheetHeader h
                WHERE h.IsDeleted = 0
                  AND (@SearchTerm IS NULL OR h.TripSheetNo LIKE '%' + @SearchTerm + '%'
                       OR h.VehicleNo LIKE '%' + @SearchTerm + '%')
                ORDER BY h.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var totalCount = await _dbConnection.ExecuteScalarAsync<int>(countSql, new { SearchTerm = searchTerm });
            var data = (await _dbConnection.QueryAsync<TripSheetHeaderDto>(dataSql, new { SearchTerm = searchTerm, Offset = offset, PageSize = pageSize })).ToList();

            // Populate UnitName via cross-module lookup
            if (data.Count > 0)
            {
                var unitIds = data.Select(d => d.UnitId).Distinct();
                var units = await _unitLookup.GetByIdsAsync(unitIds);
                var unitDict = units.ToDictionary(u => u.UnitId, u => u.UnitName);

                foreach (var item in data)
                {
                    item.UnitName = unitDict.TryGetValue(item.UnitId, out var name) ? name : null;
                }
            }

            return (data, totalCount);
        }

        public async Task<TripSheetHeaderDto?> GetByIdAsync(int id)
        {
            const string headerSql = @"
                SELECT h.Id, h.TripSheetNo, h.TripDate, h.VehicleNo, h.UnitId,
                       h.Remarks, h.IsActive, h.IsDeleted,
                       h.CreatedBy, h.CreatedDate, h.CreatedByName, h.CreatedIP,
                       h.ModifiedBy, h.ModifiedDate, h.ModifiedByName, h.ModifiedIP
                FROM Sales.TripSheetHeader h
                WHERE h.Id = @Id AND h.IsDeleted = 0";

            var header = await _dbConnection.QueryFirstOrDefaultAsync<TripSheetHeaderDto>(headerSql, new { Id = id });

            if (header == null)
                return null;

            // Populate UnitName
            var unit = await _unitLookup.GetByIdAsync(header.UnitId);
            header.UnitName = unit?.UnitName;

            // Fetch details with same-module JOINs
            const string detailSql = @"
                SELECT e.Id, e.TripSheetHeaderId, e.SequenceNo,
                       e.DispatchAdviceHeaderId,
                       d.DispatchNo, d.DispatchDate, d.VehicleNo,
                       d.TotDispatchedQty, d.PartyId, d.InvFlg,
                       so.SalesOrderNo AS OrderNo,
                       i.Id AS InvoiceHeaderId, i.InvoiceNo
                FROM Sales.TripSheetDetail e
                INNER JOIN Sales.DispatchAdviceHeader d ON e.DispatchAdviceHeaderId = d.Id AND d.IsDeleted = 0
                LEFT JOIN Sales.SalesOrderHeader so ON d.SalesOrderId = so.Id AND so.IsDeleted = 0
                LEFT JOIN Sales.InvoiceHeader i ON i.DispatchAdviceId = d.Id AND i.IsDeleted = 0
                WHERE e.TripSheetHeaderId = @TripSheetHeaderId
                ORDER BY e.SequenceNo";

            var details = (await _dbConnection.QueryAsync<TripSheetDetailDto>(detailSql, new { TripSheetHeaderId = id })).ToList();

            // Populate cross-module PartyName and City
            if (details.Count > 0)
            {
                var partyIds = details.Select(d => d.PartyId).Distinct();
                var parties = await _partyLookup.GetByIdsAsync(partyIds);
                var partyDict = parties.ToDictionary(p => p.Id);

                foreach (var detail in details)
                {
                    if (partyDict.TryGetValue(detail.PartyId, out var party))
                    {
                        detail.CustomerName = party.PartyName;
                        var shippingAddr = party.Addresses?.FirstOrDefault();
                        detail.City = shippingAddr?.City;
                    }
                }
            }

            header.TotalDispatches = details.Count;
            header.TotalInvoiced = details.Count(d => d.InvFlg);
            header.Details = details;

            return header;
        }

        public async Task<IReadOnlyList<TripSheetLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            const string sql = @"
                SELECT TOP 20 Id, TripSheetNo, VehicleNo, TripDate
                FROM Sales.TripSheetHeader
                WHERE IsActive = 1 AND IsDeleted = 0
                  AND (TripSheetNo LIKE '%' + @Term + '%'
                       OR VehicleNo LIKE '%' + @Term + '%')
                ORDER BY TripSheetNo";

            var result = await _dbConnection.QueryAsync<TripSheetLookupDto>(sql, new { Term = term });
            return result.ToList();
        }

        public async Task<bool> AlreadyExistsAsync(string tripSheetNo, int? id = null)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Sales.TripSheetHeader
                    WHERE TripSheetNo = @TripSheetNo AND IsDeleted = 0
                      AND (@Id IS NULL OR Id <> @Id)
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { TripSheetNo = tripSheetNo, Id = id });
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT CASE WHEN NOT EXISTS (
                    SELECT 1 FROM Sales.TripSheetHeader
                    WHERE Id = @Id AND IsDeleted = 0
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = id });
        }

        public async Task<bool> DispatchExistsAsync(int dispatchAdviceHeaderId)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Sales.DispatchAdviceHeader
                    WHERE Id = @Id AND IsDeleted = 0 AND IsActive = 1
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = dispatchAdviceHeaderId });
        }

        public async Task<bool> DispatchAlreadyInTripAsync(int dispatchAdviceHeaderId, int? excludeTripSheetHeaderId = null)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Sales.TripSheetDetail td
                    INNER JOIN Sales.TripSheetHeader th ON td.TripSheetHeaderId = th.Id AND th.IsDeleted = 0
                    WHERE td.DispatchAdviceHeaderId = @DispatchId
                      AND (@ExcludeId IS NULL OR td.TripSheetHeaderId <> @ExcludeId)
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { DispatchId = dispatchAdviceHeaderId, ExcludeId = excludeTripSheetHeaderId });
        }
    }
}
