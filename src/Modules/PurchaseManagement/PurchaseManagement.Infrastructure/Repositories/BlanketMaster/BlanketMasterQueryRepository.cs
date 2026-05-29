using System.Data;
using Dapper;
using PurchaseManagement.Application.BlanketMaster.Dto;
using PurchaseManagement.Application.Common.Interfaces.IBlanketMaster;

namespace PurchaseManagement.Infrastructure.Repositories.BlanketMaster;

public sealed class BlanketMasterQueryRepository : IBlanketMasterQueryRepository
{
    private readonly IDbConnection _db;

    public BlanketMasterQueryRepository(IDbConnection db)
    {
        _db = db;
    }

    public async Task<(List<BlanketHeaderDto>, int)> GetAllAsync(
        int pageNumber, int pageSize, string? searchTerm, CancellationToken ct)
    {
        const string countSql = @"
            SELECT COUNT(*)
            FROM Purchase.BlanketHeader h
            LEFT JOIN Purchase.MiscMaster ms ON h.StatusId = ms.Id AND ms.IsDeleted = 0
            LEFT JOIN Purchase.MiscMaster mp ON h.ProcurementTypeId = mp.Id AND mp.IsDeleted = 0
            WHERE h.IsDeleted = 0
              AND (@Search IS NULL OR h.BlanketNumber LIKE '%' + @Search + '%');";

        const string dataSql = @"
            SELECT h.Id, h.UnitId, h.BlanketNumber, h.BlanketDate,
                   h.VendorId, h.CurrencyId,
                   h.ProcurementTypeId, mp.Description AS ProcurementTypeName,
                   h.BrokerName, h.ValidityFrom, h.ValidityTo,
                   h.PaymentTerms, h.DeliveryTerms,
                   h.StatusId, ms.Description AS StatusName,
                   h.TotalEstimatedValue, h.Remarks,
                   h.IsActive, h.IsDeleted,
                   h.CreatedBy, h.CreatedDate, h.CreatedByName, h.CreatedIP,
                   h.ModifiedBy, h.ModifiedDate, h.ModifiedByName, h.ModifiedIP
            FROM Purchase.BlanketHeader h
            LEFT JOIN Purchase.MiscMaster ms ON h.StatusId = ms.Id AND ms.IsDeleted = 0
            LEFT JOIN Purchase.MiscMaster mp ON h.ProcurementTypeId = mp.Id AND mp.IsDeleted = 0
            WHERE h.IsDeleted = 0
              AND (@Search IS NULL OR h.BlanketNumber LIKE '%' + @Search + '%')
            ORDER BY h.Id DESC
            OFFSET @Offset ROWS FETCH NEXT @Size ROWS ONLY;";

        var offset = (pageNumber - 1) * pageSize;
        var param = new { Search = searchTerm, Offset = offset, Size = pageSize };

        var total = await _db.ExecuteScalarAsync<int>(countSql, param);
        var rows = (await _db.QueryAsync<BlanketHeaderDto>(dataSql, param)).ToList();

        return (rows, total);
    }

    public async Task<BlanketHeaderDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        const string headerSql = @"
            SELECT h.Id, h.UnitId, h.BlanketNumber, h.BlanketDate,
                   h.VendorId, h.CurrencyId,
                   h.ProcurementTypeId, mp.Description AS ProcurementTypeName,
                   h.BrokerName, h.ValidityFrom, h.ValidityTo,
                   h.PaymentTerms, h.DeliveryTerms,
                   h.StatusId, ms.Description AS StatusName,
                   h.TotalEstimatedValue, h.Remarks,
                   h.IsActive, h.IsDeleted,
                   h.CreatedBy, h.CreatedDate, h.CreatedByName, h.CreatedIP,
                   h.ModifiedBy, h.ModifiedDate, h.ModifiedByName, h.ModifiedIP
            FROM Purchase.BlanketHeader h
            LEFT JOIN Purchase.MiscMaster ms ON h.StatusId = ms.Id AND ms.IsDeleted = 0
            LEFT JOIN Purchase.MiscMaster mp ON h.ProcurementTypeId = mp.Id AND mp.IsDeleted = 0
            WHERE h.Id = @Id AND h.IsDeleted = 0;";

        const string detailSql = @"
            SELECT d.Id, d.BlanketHeaderId, d.ItemSno, d.ItemId, d.UOMId,
                   d.EstimatedQuantity, d.Rate, d.TotalPrice,
                   d.HSNId, d.GSTPercentage, d.QualitySpecification,
                   d.IsActive, d.IsDeleted
            FROM Purchase.BlanketDetail d
            WHERE d.BlanketHeaderId = @Id AND d.IsDeleted = 0
            ORDER BY d.ItemSno;";

        const string scheduleSql = @"
            SELECT s.Id, s.BlanketDetailId, s.ScheduleNo, s.ScheduleDate,
                   s.ScheduleQuantity, s.Remarks
            FROM Purchase.BlanketSchedule s
            INNER JOIN Purchase.BlanketDetail d ON s.BlanketDetailId = d.Id AND d.IsDeleted = 0
            WHERE d.BlanketHeaderId = @Id AND s.IsDeleted = 0
            ORDER BY s.ScheduleNo;";

        var header = await _db.QueryFirstOrDefaultAsync<BlanketHeaderDto>(headerSql, new { Id = id });
        if (header is null) return null;

        var details = (await _db.QueryAsync<BlanketDetailDto>(detailSql, new { Id = id })).ToList();
        var schedules = (await _db.QueryAsync<BlanketScheduleDto>(scheduleSql, new { Id = id })).ToList();

        // Assign schedules to their respective details
        var schedulesByDetail = schedules.GroupBy(s => s.BlanketDetailId)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var detail in details)
        {
            if (schedulesByDetail.TryGetValue(detail.Id, out var detailSchedules))
                detail.Schedules = detailSchedules;
        }

        header.Details = details;
        return header;
    }

    public async Task<IReadOnlyList<BlanketMasterLookupDto>> AutocompleteAsync(
        string term, bool approvedOnly, int? vendorId, DateTimeOffset? poDate, CancellationToken ct)
    {
        var sql = @"
            SELECT TOP 20
                   h.Id, h.BlanketNumber, h.VendorId,
                   h.ValidityFrom, h.ValidityTo,
                   h.TotalEstimatedValue,
                   ms.Description AS StatusName
            FROM Purchase.BlanketHeader h
            LEFT JOIN Purchase.MiscMaster ms ON h.StatusId = ms.Id AND ms.IsDeleted = 0
            WHERE h.IsDeleted = 0 AND h.IsActive = 1";

        if (approvedOnly)
            sql += " AND ms.Description = 'Approved'";

        if (vendorId.HasValue)
            sql += " AND h.VendorId = @VendorId";

        if (poDate.HasValue)
            sql += " AND h.ValidityFrom <= @PODate AND h.ValidityTo >= @PODate";

        sql += @" AND (@Term = '' OR h.BlanketNumber LIKE '%' + @Term + '%')
            ORDER BY h.BlanketNumber;";

        var results = await _db.QueryAsync<BlanketMasterLookupDto>(sql, new
        {
            Term = term,
            VendorId = vendorId,
            PODate = poDate
        });

        return results.ToList();
    }

    public async Task<bool> NotFoundAsync(int id, CancellationToken ct)
    {
        const string sql = @"
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM Purchase.BlanketHeader
                WHERE Id = @Id AND IsDeleted = 0
            ) THEN 0 ELSE 1 END;";

        return await _db.ExecuteScalarAsync<bool>(sql, new { Id = id });
    }

    public async Task<bool> AlreadyExistsAsync(string blanketNumber, int? excludeId = null)
    {
        const string sql = @"
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM Purchase.BlanketHeader
                WHERE BlanketNumber = @BlanketNumber AND IsDeleted = 0
                  AND (@ExcludeId IS NULL OR Id != @ExcludeId)
            ) THEN 1 ELSE 0 END;";

        return await _db.ExecuteScalarAsync<bool>(sql, new { BlanketNumber = blanketNumber, ExcludeId = excludeId });
    }

    public async Task<(List<GetBlanketMasterPendingGroupDto>, int)> GetBlanketMasterPendingAsync(
        int page, int size, string? search, CancellationToken ct)
    {
        const string headerSql = @"
            SELECT h.Id, h.BlanketNumber, h.BlanketDate,
                   h.VendorId, h.CurrencyId,
                   h.ValidityFrom, h.ValidityTo,
                   h.TotalEstimatedValue,
                   h.StatusId, ms.Description AS StatusName,
                   h.CreatedDate, h.CreatedByName
            FROM Purchase.BlanketHeader h
            LEFT JOIN Purchase.MiscMaster ms ON h.StatusId = ms.Id AND ms.IsDeleted = 0
            WHERE h.IsDeleted = 0
              AND ms.Description = 'Pending'
              AND (@Search IS NULL OR h.BlanketNumber LIKE '%' + @Search + '%')
            ORDER BY h.Id DESC
            OFFSET @Offset ROWS FETCH NEXT @Size ROWS ONLY;";

        const string detailSql = @"
            SELECT d.Id, d.BlanketHeaderId, d.ItemId, d.UOMId,
                   d.EstimatedQuantity, d.Rate, d.TotalPrice,
                   d.HSNId, d.GSTPercentage
            FROM Purchase.BlanketDetail d
            WHERE d.BlanketHeaderId IN @Ids AND d.IsDeleted = 0
            ORDER BY d.ItemSno;";

        var offset = (page - 1) * size;
        var headers = (await _db.QueryAsync<GetBlanketMasterPendingGroupDto>(
            headerSql, new { Search = search, Offset = offset, Size = size })).ToList();

        if (headers.Count == 0)
            return (headers, 0);

        var ids = headers.Select(h => h.Id).ToArray();
        var details = (await _db.QueryAsync<GetBlanketMasterPendingDto>(detailSql, new { Ids = ids })).ToList();

        // Group details by BlanketHeaderId (using a temp mapping via Id match)
        var detailsByHeader = details.GroupBy(d => d.Id).ToDictionary(g => g.Key, g => g.ToList());

        // We need BlanketHeaderId in the detail query. Let me fix by using a proper join
        // Actually, the detail Id is the detail's own Id. Let me re-query with BlanketHeaderId.
        const string detailWithHeaderSql = @"
            SELECT d.Id, d.BlanketHeaderId, d.ItemId, d.UOMId,
                   d.EstimatedQuantity, d.Rate, d.TotalPrice,
                   d.HSNId, d.GSTPercentage
            FROM Purchase.BlanketDetail d
            WHERE d.BlanketHeaderId IN @Ids AND d.IsDeleted = 0
            ORDER BY d.ItemSno;";

        var detailsWithHeader = (await _db.QueryAsync<dynamic>(detailWithHeaderSql, new { Ids = ids })).ToList();

        foreach (var h in headers)
        {
            h.Lines = detailsWithHeader
                .Where(d => (int)d.BlanketHeaderId == h.Id)
                .Select(d => new GetBlanketMasterPendingDto
                {
                    Id = (int)d.Id,
                    ItemId = (int)d.ItemId,
                    UOMId = (int)d.UOMId,
                    EstimatedQuantity = (decimal)d.EstimatedQuantity,
                    Rate = (decimal)d.Rate,
                    TotalPrice = (decimal)d.TotalPrice,
                    HSNId = d.HSNId != null ? (int?)d.HSNId : null,
                    GSTPercentage = d.GSTPercentage != null ? (decimal?)d.GSTPercentage : null
                })
                .ToList();
        }

        return (headers, headers.Count);
    }

    public async Task<decimal> GetPendingQuantityAsync(int blanketDetailId, CancellationToken ct)
    {
        // EstimatedQuantity - SUM(released qty from PurchaseBlanketDetail)
        const string sql = @"
            SELECT ISNULL(bd.EstimatedQuantity, 0) - ISNULL(SUM(pbd.Quantity), 0)
            FROM Purchase.BlanketDetail bd
            LEFT JOIN Purchase.PurchaseBlanketDetail pbd
                ON pbd.BlanketDetailId = bd.Id AND pbd.IsDeleted = 0
            LEFT JOIN Purchase.PurchaseBlanketHeader pbh
                ON pbd.PurchaseBlanketHeaderId = pbh.Id AND pbh.IsDeleted = 0
            LEFT JOIN Purchase.PurchaseOrderHeader poh
                ON pbh.PurchaseOrderId = poh.Id AND poh.IsDeleted = 0
            WHERE bd.Id = @BlanketDetailId AND bd.IsDeleted = 0
            GROUP BY bd.EstimatedQuantity;";

        var result = await _db.ExecuteScalarAsync<decimal?>(sql, new { BlanketDetailId = blanketDetailId });
        return result ?? 0;
    }

    public async Task<bool> IsApprovedAsync(int id, CancellationToken ct)
    {
        const string sql = @"
            SELECT CASE WHEN EXISTS (
                SELECT 1
                FROM Purchase.BlanketHeader h
                INNER JOIN Purchase.MiscMaster ms ON h.StatusId = ms.Id AND ms.IsDeleted = 0
                WHERE h.Id = @Id AND h.IsDeleted = 0 AND ms.Description = 'Approved'
            ) THEN 1 ELSE 0 END;";

        return await _db.ExecuteScalarAsync<bool>(sql, new { Id = id });
    }

    public async Task<bool> IsExpiredAsync(int id, CancellationToken ct)
    {
        const string sql = @"
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM Purchase.BlanketHeader
                WHERE Id = @Id AND IsDeleted = 0 AND ValidityTo < @Now
            ) THEN 1 ELSE 0 END;";

        return await _db.ExecuteScalarAsync<bool>(sql, new { Id = id, Now = DateTimeOffset.UtcNow });
    }

    public async Task<bool> HasOverlappingBlanketAsync(
        int vendorId, List<int> itemIds, DateTimeOffset validityFrom, DateTimeOffset validityTo, int? excludeId = null)
    {
        const string sql = @"
            SELECT CASE WHEN EXISTS (
                SELECT 1
                FROM Purchase.BlanketHeader h
                INNER JOIN Purchase.BlanketDetail d ON d.BlanketHeaderId = h.Id AND d.IsDeleted = 0
                WHERE h.IsDeleted = 0
                  AND h.VendorId = @VendorId
                  AND d.ItemId IN @ItemIds
                  AND h.ValidityFrom <= @ValidityTo
                  AND h.ValidityTo >= @ValidityFrom
                  AND (@ExcludeId IS NULL OR h.Id != @ExcludeId)
            ) THEN 1 ELSE 0 END;";

        return await _db.ExecuteScalarAsync<bool>(sql, new
        {
            VendorId = vendorId,
            ItemIds = itemIds,
            ValidityFrom = validityFrom,
            ValidityTo = validityTo,
            ExcludeId = excludeId
        });
    }
}
