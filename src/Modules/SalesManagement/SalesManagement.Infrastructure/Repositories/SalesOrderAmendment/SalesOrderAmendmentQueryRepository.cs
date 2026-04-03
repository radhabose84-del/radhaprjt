using System.Data;
using Contracts.Interfaces.Lookups.Inventory;
using Dapper;
using SalesManagement.Application.Common.Interfaces.ISalesOrderAmendment;
using SalesManagement.Application.SalesOrder.Dto;
using SalesManagement.Application.SalesOrder.Queries.GetPendingSalesOrderAmendment;

namespace SalesManagement.Infrastructure.Repositories.SalesOrderAmendment
{
    public class SalesOrderAmendmentQueryRepository : ISalesOrderAmendmentQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IItemLookup _itemLookup;

        public SalesOrderAmendmentQueryRepository(IDbConnection dbConnection, IItemLookup itemLookup)
        {
            _dbConnection = dbConnection;
            _itemLookup = itemLookup;
        }

        public async Task<(List<SalesOrderAmendmentHeaderDto>, int)> GetAllAsync(
            int pageNumber, int pageSize, string? searchTerm)
        {
            var searchFilter = string.IsNullOrWhiteSpace(searchTerm)
                ? ""
                : "AND (ah.AmendmentNo LIKE @Search OR soh.SalesOrderNo LIKE @Search)";

            var sql = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Sales.SalesOrderAmendmentHeader ah
                INNER JOIN Sales.SalesOrderHeader soh ON ah.SalesOrderHeaderId = soh.Id
                WHERE ah.IsDeleted = 0 {searchFilter};

                SELECT
                    ah.Id, ah.SalesOrderHeaderId, ah.UnitId,
                    soh.SalesOrderNo,
                    ah.AmendmentNo, ah.RevisionNumber,
                    ah.AmendmentDate, ah.Reason,
                    ah.StatusId,
                    mm.Description AS StatusName,
                    ah.ApprovedBy, ah.ApprovedDate,
                    ah.CreatedByName, ah.CreatedDate
                FROM Sales.SalesOrderAmendmentHeader ah
                INNER JOIN Sales.SalesOrderHeader soh ON ah.SalesOrderHeaderId = soh.Id
                LEFT JOIN Sales.MiscMaster mm ON ah.StatusId = mm.Id AND mm.IsDeleted = 0
                WHERE ah.IsDeleted = 0 {searchFilter}
                ORDER BY ah.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount;";

            var offset = (pageNumber - 1) * pageSize;
            var param = new
            {
                Offset = offset,
                PageSize = pageSize,
                Search = $"%{searchTerm}%"
            };

            using var multi = await _dbConnection.QueryMultipleAsync(sql, param);
            var items = (await multi.ReadAsync<SalesOrderAmendmentHeaderDto>()).ToList();
            var totalCount = await multi.ReadFirstOrDefaultAsync<int>();

            return (items, totalCount);
        }

        public async Task<SalesOrderAmendmentHeaderDto?> GetByIdAsync(int id)
        {
            var sql = @"
                SELECT
                    ah.Id, ah.SalesOrderHeaderId, ah.UnitId,
                    soh.SalesOrderNo,
                    ah.AmendmentNo, ah.RevisionNumber,
                    ah.AmendmentDate, ah.Reason,
                    ah.StatusId,
                    mm.Description AS StatusName,
                    ah.ApprovedBy, ah.ApprovedDate,
                    ah.CreatedByName, ah.CreatedDate
                FROM Sales.SalesOrderAmendmentHeader ah
                INNER JOIN Sales.SalesOrderHeader soh ON ah.SalesOrderHeaderId = soh.Id
                LEFT JOIN Sales.MiscMaster mm ON ah.StatusId = mm.Id AND mm.IsDeleted = 0
                WHERE ah.Id = @Id AND ah.IsDeleted = 0;

                SELECT
                    d.Id, d.SalesOrderAmendmentHeaderId,
                    d.ChangeType, d.SalesOrderDetailId,
                    d.OldItemId, d.OldQtyInBags, d.OldExMillRate, d.OldExpectedDeliveryDate,
                    d.NewQtyInBags, d.NewExMillRate, d.NewExpectedDeliveryDate
                FROM Sales.SalesOrderAmendmentDetail d
                WHERE d.SalesOrderAmendmentHeaderId = @Id;";

            using var multi = await _dbConnection.QueryMultipleAsync(sql, new { Id = id });
            var header = await multi.ReadFirstOrDefaultAsync<SalesOrderAmendmentHeaderDto>();
            if (header == null) return null;

            var details = (await multi.ReadAsync<SalesOrderAmendmentDetailDto>()).ToList();
            await PopulateDetailLookupsAsync(details);
            header.SalesOrderAmendmentDetails = details;

            return header;
        }

        public async Task<List<SalesOrderAmendmentHeaderDto>> GetBySalesOrderHeaderIdAsync(int salesOrderHeaderId)
        {
            var sql = @"
                SELECT
                    ah.Id, ah.SalesOrderHeaderId, ah.UnitId,
                    soh.SalesOrderNo,
                    ah.AmendmentNo, ah.RevisionNumber,
                    ah.AmendmentDate, ah.Reason,
                    ah.StatusId,
                    mm.Description AS StatusName,
                    ah.ApprovedBy, ah.ApprovedDate,
                    ah.CreatedByName, ah.CreatedDate
                FROM Sales.SalesOrderAmendmentHeader ah
                INNER JOIN Sales.SalesOrderHeader soh ON ah.SalesOrderHeaderId = soh.Id
                LEFT JOIN Sales.MiscMaster mm ON ah.StatusId = mm.Id AND mm.IsDeleted = 0
                WHERE ah.SalesOrderHeaderId = @SalesOrderHeaderId AND ah.IsDeleted = 0
                ORDER BY ah.Id DESC;

                SELECT
                    d.Id, d.SalesOrderAmendmentHeaderId,
                    d.ChangeType, d.SalesOrderDetailId,
                    d.OldItemId, d.OldQtyInBags, d.OldExMillRate, d.OldExpectedDeliveryDate,
                    d.NewQtyInBags, d.NewExMillRate, d.NewExpectedDeliveryDate
                FROM Sales.SalesOrderAmendmentDetail d
                INNER JOIN Sales.SalesOrderAmendmentHeader ah ON d.SalesOrderAmendmentHeaderId = ah.Id
                WHERE ah.SalesOrderHeaderId = @SalesOrderHeaderId AND ah.IsDeleted = 0;";

            using var multi = await _dbConnection.QueryMultipleAsync(sql, new { SalesOrderHeaderId = salesOrderHeaderId });
            var headers = (await multi.ReadAsync<SalesOrderAmendmentHeaderDto>()).ToList();
            var allDetails = (await multi.ReadAsync<SalesOrderAmendmentDetailDto>()).ToList();

            await PopulateDetailLookupsAsync(allDetails);

            foreach (var header in headers)
            {
                header.SalesOrderAmendmentDetails = allDetails
                    .Where(d => d.SalesOrderAmendmentHeaderId == header.Id)
                    .ToList();
            }

            return headers;
        }

        private async Task PopulateDetailLookupsAsync(List<SalesOrderAmendmentDetailDto> details)
        {
            if (details.Count == 0) return;

            var itemIds = details.Select(d => d.OldItemId).Distinct();
            var items = await _itemLookup.GetByIdsAsync(itemIds);
            var itemDict = items.ToDictionary(i => i.Id, i => i.ItemName);

            foreach (var d in details)
            {
                d.OldItemName = itemDict.TryGetValue(d.OldItemId, out var name) ? name : null;

                if (d.ChangeType == "Removed")
                {
                    d.Remarks = "Item Removed";
                }
                else
                {
                    var changes = new List<string>();
                    if (d.NewQtyInBags.HasValue) changes.Add("Qty Amendment");
                    if (d.NewExMillRate.HasValue) changes.Add("ExMill Rate Amendment");
                    if (d.NewExpectedDeliveryDate.HasValue) changes.Add("Expected Delivery Amendment");
                    d.Remarks = changes.Count > 0 ? string.Join(" / ", changes) : null;
                }
            }
        }

        public async Task<(List<PendingSalesOrderAmendmentDto>, int)> GetPendingAsync(
            int pageNumber, int pageSize, string? searchTerm)
        {
            var searchFilter = string.IsNullOrWhiteSpace(searchTerm)
                ? ""
                : "AND (ah.AmendmentNo LIKE @Search OR soh.SalesOrderNo LIKE @Search)";

            var sql = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Sales.SalesOrderAmendmentHeader ah
                INNER JOIN Sales.SalesOrderHeader soh ON ah.SalesOrderHeaderId = soh.Id
                INNER JOIN Sales.MiscMaster mm ON ah.StatusId = mm.Id AND mm.IsDeleted = 0
                WHERE ah.IsDeleted = 0 AND mm.Code = 'Pending' {searchFilter};

                SELECT
                    ah.Id, ah.SalesOrderHeaderId, ah.UnitId,
                    soh.SalesOrderNo,
                    ah.AmendmentNo, ah.RevisionNumber,
                    ah.AmendmentDate, ah.Reason,
                    ah.StatusId,
                    mm.Description AS StatusName,
                    ah.CreatedByName, ah.CreatedDate
                FROM Sales.SalesOrderAmendmentHeader ah
                INNER JOIN Sales.SalesOrderHeader soh ON ah.SalesOrderHeaderId = soh.Id
                INNER JOIN Sales.MiscMaster mm ON ah.StatusId = mm.Id AND mm.IsDeleted = 0
                WHERE ah.IsDeleted = 0 AND mm.Code = 'Pending' {searchFilter}
                ORDER BY ah.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount;";

            var offset = (pageNumber - 1) * pageSize;
            var param = new
            {
                Offset = offset,
                PageSize = pageSize,
                Search = $"%{searchTerm}%"
            };

            using var multi = await _dbConnection.QueryMultipleAsync(sql, param);
            var items = (await multi.ReadAsync<PendingSalesOrderAmendmentDto>()).ToList();
            var totalCount = await multi.ReadFirstOrDefaultAsync<int>();

            return (items, totalCount);
        }

        public async Task<bool> SalesOrderExistsAndApprovedAsync(int salesOrderHeaderId)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1
                    FROM Sales.SalesOrderHeader soh
                    INNER JOIN Sales.MiscMaster mm ON soh.StatusId = mm.Id AND mm.IsDeleted = 0
                    WHERE soh.Id = @Id AND soh.IsDeleted = 0 AND mm.Code = 'Approved'
                ) THEN 1 ELSE 0 END;";
            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = salesOrderHeaderId });
        }

        public async Task<bool> HasDispatchAdviceAsync(int salesOrderHeaderId)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Sales.DispatchAdviceHeader
                    WHERE SalesOrderId = @Id AND IsDeleted = 0
                ) THEN 1 ELSE 0 END;";
            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = salesOrderHeaderId });
        }

        public async Task<bool> HasPendingAmendmentAsync(int salesOrderHeaderId)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1
                    FROM Sales.SalesOrderAmendmentHeader ah
                    INNER JOIN Sales.MiscMaster mm ON ah.StatusId = mm.Id AND mm.IsDeleted = 0
                    WHERE ah.SalesOrderHeaderId = @Id AND ah.IsDeleted = 0 AND mm.Code = 'Pending'
                ) THEN 1 ELSE 0 END;";
            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = salesOrderHeaderId });
        }

        public async Task<bool> SalesOrderDetailExistsAsync(int salesOrderDetailId, int salesOrderHeaderId)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Sales.SalesOrderDetail
                    WHERE Id = @DetailId AND SalesOrderHeaderId = @HeaderId
                ) THEN 1 ELSE 0 END;";
            return await _dbConnection.ExecuteScalarAsync<bool>(sql,
                new { DetailId = salesOrderDetailId, HeaderId = salesOrderHeaderId });
        }
    }
}
