using System.Data;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Production;
using Contracts.Interfaces.Lookups.Warehouse;
using Dapper;
using SalesManagement.Application.Common.Interfaces.ISalesReturn;
using SalesManagement.Application.SalesReturn.Dto;

namespace SalesManagement.Infrastructure.Repositories.SalesReturn
{
    public class SalesReturnQueryRepository : ISalesReturnQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IPartyLookup _partyLookup;
        private readonly IItemLookup _itemLookup;
        private readonly ILotMasterLookup _lotLookup;
        private readonly IWarehouseLookup _warehouseLookup;
        private readonly IBinLookup _binLookup;

        public SalesReturnQueryRepository(
            IDbConnection dbConnection,
            IPartyLookup partyLookup,
            IItemLookup itemLookup,
            ILotMasterLookup lotLookup,
            IWarehouseLookup warehouseLookup,
            IBinLookup binLookup)
        {
            _dbConnection = dbConnection;
            _partyLookup = partyLookup;
            _itemLookup = itemLookup;
            _lotLookup = lotLookup;
            _warehouseLookup = warehouseLookup;
            _binLookup = binLookup;
        }

        public async Task<(List<SalesReturnListDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var searchFilter = string.IsNullOrWhiteSpace(searchTerm)
                ? string.Empty
                : @" AND (h.ReturnNumber LIKE @SearchTerm OR ch.ComplaintNumber LIKE @SearchTerm OR h.Remarks LIKE @SearchTerm)";

            var countSql = $@"
                SELECT COUNT(*)
                FROM Sales.SalesReturnHeader h
                INNER JOIN Sales.ComplaintHeader ch ON h.ComplaintHeaderId = ch.Id AND ch.IsDeleted = 0
                WHERE h.IsDeleted = 0 {searchFilter};";

            var dataSql = $@"
                SELECT
                    h.Id,
                    h.ReturnNumber,
                    h.ReturnDate,
                    h.ComplaintHeaderId,
                    ch.ComplaintNumber,
                    h.CustomerId,
                    h.WarehouseId,
                    cr.ResolutionTypeId,
                    rt.Description AS ResolutionTypeName,
                    ms.Description AS StatusName,
                    (SELECT COUNT(*) FROM Sales.SalesReturnDetail d WHERE d.SalesReturnHeaderId = h.Id AND d.IsDeleted = 0) AS DetailCount
                FROM Sales.SalesReturnHeader h
                INNER JOIN Sales.ComplaintHeader ch ON h.ComplaintHeaderId = ch.Id AND ch.IsDeleted = 0
                LEFT JOIN Sales.ComplaintResolution cr ON cr.ComplaintHeaderId = ch.Id AND cr.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster rt ON cr.ResolutionTypeId = rt.Id AND rt.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster ms ON h.StatusId = ms.Id AND ms.IsDeleted = 0
                WHERE h.IsDeleted = 0 {searchFilter}
                ORDER BY h.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

            var parameters = new
            {
                SearchTerm = $"%{searchTerm}%",
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            var totalCount = await _dbConnection.ExecuteScalarAsync<int>(countSql, parameters);
            var data = (await _dbConnection.QueryAsync<SalesReturnListDto>(dataSql, parameters)).ToList();

            if (data.Count > 0)
            {
                // Populate CustomerName
                var customerIds = data.Select(d => d.CustomerId).Distinct();
                var parties = await _partyLookup.GetByIdsAsync(customerIds);
                var partyDict = parties.ToDictionary(p => p.Id, p => p.PartyName);

                // Populate WarehouseName
                var warehouseIds = data.Where(d => d.WarehouseId > 0).Select(d => d.WarehouseId).Distinct();
                var warehouses = await _warehouseLookup.GetByIdsAsync(warehouseIds);
                var whDict = warehouses.ToDictionary(w => w.Id, w => w.WarehouseName);

                foreach (var item in data)
                {
                    item.CustomerName = partyDict.TryGetValue(item.CustomerId, out var name) ? name : null;
                    item.WarehouseName = item.WarehouseId > 0 && whDict.TryGetValue(item.WarehouseId, out var whName) ? whName : null;
                }
            }

            return (data, totalCount);
        }

        public async Task<SalesReturnHeaderDto?> GetByIdAsync(int id)
        {
            return await GetReturnAsync("h.Id = @Id", new { Id = id });
        }

        public async Task<SalesReturnHeaderDto?> GetByComplaintIdAsync(int complaintHeaderId)
        {
            return await GetReturnAsync("h.ComplaintHeaderId = @ComplaintHeaderId", new { ComplaintHeaderId = complaintHeaderId });
        }

        public async Task<List<SalesReturnHeaderDto>> GetAllByComplaintIdAsync(int complaintHeaderId)
        {
            const string idsSql = @"
                SELECT Id FROM Sales.SalesReturnHeader
                WHERE ComplaintHeaderId = @ComplaintHeaderId AND IsDeleted = 0
                ORDER BY Id DESC;";

            var ids = (await _dbConnection.QueryAsync<int>(idsSql, new { ComplaintHeaderId = complaintHeaderId })).ToList();

            var result = new List<SalesReturnHeaderDto>();
            foreach (var id in ids)
            {
                var item = await GetReturnAsync("h.Id = @Id", new { Id = id });
                if (item != null)
                    result.Add(item);
            }

            return result;
        }

        public async Task<ComplaintReturnDataDto?> GetComplaintReturnDataAsync(int complaintHeaderId)
        {
            // Get complaint header info
            const string headerSql = @"
                SELECT
                    ch.Id AS ComplaintHeaderId,
                    ch.ComplaintNumber,
                    ch.ComplaintDate,
                    ch.CustomerId,
                    rt.Description AS ResolutionType
                FROM Sales.ComplaintHeader ch
                LEFT JOIN Sales.ComplaintResolution cr ON cr.ComplaintHeaderId = ch.Id AND cr.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster rt ON cr.ResolutionTypeId = rt.Id AND rt.IsDeleted = 0
                WHERE ch.Id = @Id AND ch.IsDeleted = 0;";

            var header = await _dbConnection.QueryFirstOrDefaultAsync<ComplaintReturnDataDto>(headerSql, new { Id = complaintHeaderId });
            if (header == null) return null;

            // Populate CustomerName
            if (header.CustomerId > 0)
            {
                var party = await _partyLookup.GetByIdAsync(header.CustomerId);
                header.CustomerName = party?.PartyName;
            }

            // Get complaint detail items with invoice info and actual InvoiceDetail.Id
            const string detailSql = @"
                SELECT
                    cd.InvoiceHeaderId,
                    ih.InvoiceNo,
                    ih.InvoiceDate,
                    id.Id AS InvoiceDetailId,
                    cd.ItemId,
                    cd.LotId,
                    cd.NumberOfPacks,
                    cd.NetWeight,
                    cd.InvoiceAmount
                FROM Sales.ComplaintDetail cd
                INNER JOIN Sales.InvoiceHeader ih ON cd.InvoiceHeaderId = ih.Id AND ih.IsDeleted = 0
                LEFT JOIN Sales.InvoiceDetail id ON id.InvoiceHeaderId = ih.Id AND id.ItemId = cd.ItemId
                    AND (id.LotId = cd.LotId OR (id.LotId IS NULL AND cd.LotId IS NULL))
                WHERE cd.ComplaintHeaderId = @ComplaintHeaderId AND cd.IsDeleted = 0;";

            var details = (await _dbConnection.QueryAsync<ComplaintInvoiceItemDto>(detailSql, new { ComplaintHeaderId = complaintHeaderId })).ToList();

            if (details.Count > 0)
            {
                // Populate Item names
                var itemIds = details.Select(d => d.ItemId).Distinct();
                var items = await _itemLookup.GetByIdsAsync(itemIds);
                var itemDict = items.ToDictionary(i => i.Id, i => (i.ItemCode, i.ItemName));

                // Populate Lot codes
                var lotIds = details.Where(d => d.LotId.HasValue).Select(d => d.LotId!.Value).Distinct();
                if (lotIds.Any())
                {
                    var lots = await _lotLookup.GetByIdsAsync(lotIds);
                    var lotDict = lots.ToDictionary(l => l.Id, l => l.LotCode);
                    foreach (var detail in details.Where(d => d.LotId.HasValue))
                    {
                        if (lotDict.TryGetValue(detail.LotId!.Value, out var lotCode))
                            detail.LotCode = lotCode;
                    }
                }

                foreach (var detail in details)
                {
                    if (itemDict.TryGetValue(detail.ItemId, out var itemInfo))
                    {
                        detail.ItemCode = itemInfo.ItemCode;
                        detail.ItemName = itemInfo.ItemName;
                    }
                }

                // Get dispatch pack ranges for each invoice detail
                var invoiceDetailIds = details.Select(d => d.InvoiceDetailId).Distinct().ToList();
                const string dispatchSql = @"
                    SELECT
                        id.Id AS InvoiceDetailId,
                        da.StartPackNo AS DispatchStartPackNo,
                        da.EndPackNo AS DispatchEndPackNo,
                        da.PackTypeId
                    FROM Sales.InvoiceDetail id
                    INNER JOIN Sales.InvoiceHeader ih ON id.InvoiceHeaderId = ih.Id
                    INNER JOIN Sales.DispatchAdviceHeader dah ON ih.DispatchAdviceId = dah.Id
                    INNER JOIN Sales.DispatchAdviceDetail da ON da.DispatchAdviceHeaderId = dah.Id
                        AND da.ItemId = id.ItemId
                        AND (da.LotId = id.LotId OR (da.LotId IS NULL AND id.LotId IS NULL))
                    WHERE id.Id IN @InvoiceDetailIds;";

                var dispatchData = (await _dbConnection.QueryAsync<dynamic>(dispatchSql, new { InvoiceDetailIds = invoiceDetailIds })).ToList();

                foreach (var detail in details)
                {
                    var dispatch = dispatchData.FirstOrDefault(d => (int)d.InvoiceDetailId == detail.InvoiceDetailId);
                    if (dispatch != null)
                    {
                        detail.DispatchStartPackNo = (int)dispatch.DispatchStartPackNo;
                        detail.DispatchEndPackNo = (int)dispatch.DispatchEndPackNo;
                        detail.PackTypeId = (int?)dispatch.PackTypeId;
                    }
                }

                // Get already returned pack ranges
                const string returnedSql = @"
                    SELECT InvoiceDetailId, StartPackNo, EndPackNo
                    FROM Sales.SalesReturnDetail
                    WHERE InvoiceDetailId IN @InvoiceDetailIds AND IsDeleted = 0;";

                var returnedRanges = (await _dbConnection.QueryAsync<(int InvoiceDetailId, int StartPackNo, int EndPackNo)>(
                    returnedSql, new { InvoiceDetailIds = invoiceDetailIds })).ToList();

                foreach (var detail in details)
                {
                    detail.ReturnedPackRanges = returnedRanges
                        .Where(r => r.InvoiceDetailId == detail.InvoiceDetailId)
                        .Select(r => new PackRangeDto { StartPackNo = r.StartPackNo, EndPackNo = r.EndPackNo })
                        .ToList();
                }
            }

            header.InvoiceItems = details;

            // Fetch bag status lookup
            const string bagStatusSql = @"
                SELECT mm.Id, mm.Code, mm.Description
                FROM Sales.MiscMaster mm
                INNER JOIN Sales.MiscTypeMaster mt ON mm.MiscTypeId = mt.Id
                WHERE mt.miscTypecode = 'BagStatus' AND mm.IsActive = 1 AND mm.IsDeleted = 0
                ORDER BY mm.Description;";
            header.BagStatuses = [.. await _dbConnection.QueryAsync<BagStatusLookupDto>(bagStatusSql)];

            return header;
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = "SELECT COUNT(1) FROM Sales.SalesReturnHeader WHERE Id = @Id AND IsDeleted = 0;";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<bool> ComplaintExistsAsync(int complaintHeaderId)
        {
            const string sql = "SELECT COUNT(1) FROM Sales.ComplaintHeader WHERE Id = @Id AND IsDeleted = 0;";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = complaintHeaderId });
            return count > 0;
        }

        public async Task<bool> IsComplaintReturnEligibleAsync(int complaintHeaderId)
        {
            // Check if complaint has a resolution with type = Sales Return
            const string sql = @"
                SELECT COUNT(1)
                FROM Sales.ComplaintResolution cr
                INNER JOIN Sales.MiscMaster rt ON cr.ResolutionTypeId = rt.Id AND rt.IsDeleted = 0
                WHERE cr.ComplaintHeaderId = @Id AND cr.IsDeleted = 0
                    AND rt.Code = 'Sales Return';";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = complaintHeaderId });
            return count > 0;
        }

        public async Task<bool> PackRangeOverlapsAsync(int invoiceDetailId, int startPackNo, int endPackNo, int? excludeReturnHeaderId = null)
        {
            var excludeFilter = excludeReturnHeaderId.HasValue
                ? " AND SalesReturnHeaderId != @ExcludeId"
                : string.Empty;

            var sql = $@"
                SELECT COUNT(1) FROM Sales.SalesReturnDetail
                WHERE InvoiceDetailId = @InvoiceDetailId AND IsDeleted = 0
                    AND StartPackNo <= @EndPackNo AND EndPackNo >= @StartPackNo {excludeFilter};";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new
            {
                InvoiceDetailId = invoiceDetailId,
                StartPackNo = startPackNo,
                EndPackNo = endPackNo,
                ExcludeId = excludeReturnHeaderId
            });
            return count > 0;
        }

        public async Task<bool> PackRangeExistsInDispatchAsync(int invoiceDetailId, int startPackNo, int endPackNo)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Sales.InvoiceDetail id
                INNER JOIN Sales.InvoiceHeader ih ON id.InvoiceHeaderId = ih.Id
                INNER JOIN Sales.DispatchAdviceHeader dah ON ih.DispatchAdviceId = dah.Id
                INNER JOIN Sales.DispatchAdviceDetail da ON da.DispatchAdviceHeaderId = dah.Id
                    AND da.ItemId = id.ItemId
                WHERE id.Id = @InvoiceDetailId
                    AND da.StartPackNo <= @StartPackNo AND da.EndPackNo >= @EndPackNo;";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new
            {
                InvoiceDetailId = invoiceDetailId,
                StartPackNo = startPackNo,
                EndPackNo = endPackNo
            });
            return count > 0;
        }

        public async Task<(int TotalDispatchedPacks, int TotalReturnedPacks)> GetReturnProgressAsync(int complaintHeaderId)
        {
            const string sql = @"
                SELECT
                    ISNULL(SUM(da.EndPackNo - da.StartPackNo + 1), 0) AS TotalDispatchedPacks
                FROM Sales.ComplaintDetail cd
                INNER JOIN Sales.InvoiceHeader ih ON cd.InvoiceHeaderId = ih.Id AND ih.IsDeleted = 0
                INNER JOIN Sales.DispatchAdviceHeader dah ON ih.DispatchAdviceId = dah.Id AND dah.IsDeleted = 0
                INNER JOIN Sales.DispatchAdviceDetail da ON da.DispatchAdviceHeaderId = dah.Id AND da.ItemId = cd.ItemId
                WHERE cd.ComplaintHeaderId = @ComplaintHeaderId AND cd.IsDeleted = 0;

                SELECT
                    ISNULL(SUM(srd.EndPackNo - srd.StartPackNo + 1), 0) AS TotalReturnedPacks
                FROM Sales.SalesReturnDetail srd
                INNER JOIN Sales.SalesReturnHeader srh ON srd.SalesReturnHeaderId = srh.Id AND srh.IsDeleted = 0
                WHERE srh.ComplaintHeaderId = @ComplaintHeaderId AND srd.IsDeleted = 0;";

            using var multi = await _dbConnection.QueryMultipleAsync(sql, new { ComplaintHeaderId = complaintHeaderId });
            var dispatched = await multi.ReadFirstAsync<int>();
            var returned = await multi.ReadFirstAsync<int>();

            return (dispatched, returned);
        }

        private async Task<SalesReturnHeaderDto?> GetReturnAsync(string whereClause, object parameters)
        {
            var sql = $@"
                SELECT
                    h.Id, h.ReturnNumber, h.ReturnDate,
                    h.ComplaintHeaderId, ch.ComplaintNumber,
                    h.CustomerId, h.WarehouseId, h.BinId,
                    h.StatusId, ms.Description AS StatusName,
                    h.Remarks, h.IsActive,
                    h.CreatedBy, h.CreatedByName, h.CreatedDate,
                    h.ModifiedBy, h.ModifiedByName, h.ModifiedDate
                FROM Sales.SalesReturnHeader h
                INNER JOIN Sales.ComplaintHeader ch ON h.ComplaintHeaderId = ch.Id AND ch.IsDeleted = 0
                LEFT JOIN Sales.MiscMaster ms ON h.StatusId = ms.Id AND ms.IsDeleted = 0
                WHERE h.IsDeleted = 0 AND {whereClause};";

            var header = await _dbConnection.QueryFirstOrDefaultAsync<SalesReturnHeaderDto>(sql, parameters);
            if (header == null) return null;

            // Populate cross-module lookups
            if (header.CustomerId > 0)
            {
                var party = await _partyLookup.GetByIdAsync(header.CustomerId);
                header.CustomerName = party?.PartyName;
            }

            var warehouses = await _warehouseLookup.GetByIdsAsync(new[] { header.WarehouseId });
            header.WarehouseName = warehouses.FirstOrDefault()?.WarehouseName;

            var bins = await _binLookup.GetByIdsAsync(new[] { header.BinId });
            header.BinName = bins.FirstOrDefault()?.BinName;

            // Get details
            const string detailSql = @"
                SELECT
                    d.Id, d.SalesReturnHeaderId,
                    d.InvoiceHeaderId, ih.InvoiceNo, ih.InvoiceDate,
                    d.InvoiceDetailId, d.ItemId, d.LotId,
                    d.StartPackNo, d.EndPackNo, d.ReturnQty,
                    d.PackTypeId, d.BagStatusId,
                    bs.Description AS BagStatusName
                FROM Sales.SalesReturnDetail d
                INNER JOIN Sales.InvoiceHeader ih ON d.InvoiceHeaderId = ih.Id
                LEFT JOIN Sales.MiscMaster bs ON d.BagStatusId = bs.Id AND bs.IsDeleted = 0
                WHERE d.SalesReturnHeaderId = @HeaderId AND d.IsDeleted = 0;";

            var details = (await _dbConnection.QueryAsync<SalesReturnDetailDto>(detailSql, new { HeaderId = header.Id })).ToList();

            if (details.Count > 0)
            {
                var itemIds = details.Select(d => d.ItemId).Distinct();
                var items = await _itemLookup.GetByIdsAsync(itemIds);
                var itemDict = items.ToDictionary(i => i.Id, i => (i.ItemCode, i.ItemName));

                var lotIds = details.Where(d => d.LotId.HasValue).Select(d => d.LotId!.Value).Distinct();
                if (lotIds.Any())
                {
                    var lots = await _lotLookup.GetByIdsAsync(lotIds);
                    var lotDict = lots.ToDictionary(l => l.Id, l => l.LotCode);
                    foreach (var detail in details.Where(d => d.LotId.HasValue))
                    {
                        if (lotDict.TryGetValue(detail.LotId!.Value, out var lotCode))
                            detail.LotCode = lotCode;
                    }
                }

                foreach (var detail in details)
                {
                    if (itemDict.TryGetValue(detail.ItemId, out var itemInfo))
                    {
                        detail.ItemCode = itemInfo.ItemCode;
                        detail.ItemName = itemInfo.ItemName;
                    }
                }
            }

            header.InvoiceDetails = details
                .GroupBy(d => d.InvoiceHeaderId)
                .Select(g => new SalesReturnInvoiceResponseDto
                {
                    InvoiceHeaderId = g.Key,
                    InvoiceNo = g.First().InvoiceNo,
                    InvoiceDate = g.First().InvoiceDate,
                    Items = g.ToList()
                })
                .ToList();

            return header;
        }
    }
}
