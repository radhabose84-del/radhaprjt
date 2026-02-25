using System.Data;
using InventoryManagement.Application.Common.Interfaces;
using InventoryManagement.Application.Common.Interfaces.IIssue;
using InventoryManagement.Application.Issue.Queries.GetApprovedMrsById;
using InventoryManagement.Application.Issue.Queries.GetPendingIssue;
using InventoryManagement.Application.Issue.Queries.GetPendingIssueHeader;
using InventoryManagement.Domain.Common;
using Dapper;
using static InventoryManagement.Application.Issue.Queries.GetPendingIssue.GetPendingIssueDto;

namespace InventoryManagement.Infrastructure.Repositories.Issue
{
    public class IssueEntryQueryRepository : IIssueQueryCommandRepository
    {
        
        private readonly IDbConnection _dbConnection;
        private readonly IIPAddressService _ipAddressService;
         public IssueEntryQueryRepository(IDbConnection dbConnection, IIPAddressService ipAddressService)
        {
            _dbConnection = dbConnection;
            _ipAddressService = ipAddressService;
        }

        public async Task<List<GetApprovedMrsByIdDto>> GetApprovedMrsDetails(string? searchPattern)
        {
             var unitId = _ipAddressService.GetUnitId();
            var sql = @"
                SELECT 
                    a.Id AS MrsId,
                    a.MrsNo
                FROM Inventory.MrsHeader a
                INNER JOIN Inventory.MiscMaster mm 
                    ON a.StatusId = mm.Id
                WHERE mm.Description = @Status and a.UnitId = @UnitId
            ";

            // ✅ Optional search filter
            if (!string.IsNullOrEmpty(searchPattern))
            {
                sql += " AND (a.MrsNo LIKE @SearchPattern)";
            }

            // ✅ Final order
            sql += " ORDER BY a.Id ASC;";

            var parameters = new
            {
                Status = MiscEnumEntity.Approved,
                SearchPattern = $"%{searchPattern}%",
                UnitId = unitId
            };

            var result = await _dbConnection.QueryAsync<GetApprovedMrsByIdDto>(sql, parameters);

            return result.ToList();
        }

        public async Task<string?> GetDescriptionByIdAsync(int id)
        {
             var sql = "SELECT Description FROM Inventory.MiscMaster WHERE Id = @Id";
             return await _dbConnection.QueryFirstOrDefaultAsync<string?>(sql, new { Id = id });
        }

        public async Task<List<GetPendingIssueDto.GetPendingStockBinDto>> GetMainStoresStockBinWise(List<int> itemIds, int warehouseId)
        {
             var unitId = _ipAddressService.GetUnitId();

            // Base SQL
            var sql = @"
                SELECT 
                    ItemId,
                    WarehouseId,
                    StorageTypeId,
                    TargetId,
                    UomId,
                    SUM(ReceivedQty - IssueQty) AS CurrentStockQty,
                    SUM(ReceivedValue - IssueValue) AS CurrentStockValue,
                    CASE 
                        WHEN SUM(ReceivedQty - IssueQty) <> 0 
                        THEN SUM(ReceivedValue - IssueValue) / SUM(ReceivedQty - IssueQty)
                        ELSE 0 
                    END AS AvgRate
                FROM Inventory.StockLedger
                WHERE UnitId = @UnitId
            ";

            // Apply optional filters dynamically
            if (itemIds != null && itemIds.Any())
                sql += " AND ItemId IN @ItemIds";

            if (warehouseId > 0)
                sql += " AND WarehouseId = @WarehouseId";

            sql += @"
                GROUP BY UnitId, ItemId, WarehouseId, StorageTypeId, TargetId, UomId
                HAVING SUM(ReceivedQty - IssueQty) <> 0;
            ";

            // Execute query
            var result = await _dbConnection.QueryAsync<GetPendingStockBinDto>(
                sql,
                new
                {
                    UnitId = unitId,
                    ItemIds = itemIds,
                    WarehouseId = warehouseId
                }
            );

            return result.ToList();
        }


        public async Task<(List<GetPendingIssueHeaderDto>, int)> GetPendingIssueHeaderAsync(DateTimeOffset? fromDate, DateTimeOffset? toDate, int PageNumber, int PageSize, string? SearchTerm)
        {
            var UnitId = _ipAddressService.GetUnitId();

            var parameters = new DynamicParameters();
            parameters.Add("UnitId", UnitId);
            parameters.Add("Status", MiscEnumEntity.Approved);

            // --- WHERE clause ---
            var whereClause = "WHERE A.UnitId = @UnitId AND C.Description = @Status";

            if (fromDate.HasValue)
            {
                whereClause += " AND A.MrsDate >= @FromDate";
                parameters.Add("FromDate", fromDate.Value);
            }

            if (toDate.HasValue)
            {
                var toDateEnd = toDate.Value.Date.AddDays(1).AddTicks(-1);
                whereClause += " AND A.MrsDate <= @ToDate";
                parameters.Add("ToDate", toDateEnd);
            }

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                whereClause += " AND (A.MrsNo LIKE @SearchTerm OR CAST(A.Id AS VARCHAR) LIKE @SearchTerm)";
                parameters.Add("SearchTerm", $"%{SearchTerm}%");
            }

            // Pagination
            var offset = (PageNumber - 1) * PageSize;
            parameters.Add("Offset", offset);
            parameters.Add("PageSize", PageSize);

            // --- SQL Query with CTE ---
            var query = $@"
                    -- Paginated query
                    WITH IssuedCTE AS
                    (
                        SELECT 
                            IHD.MrsHeaderId,
                            IDT.ItemId,
                            SUM(ISNULL(IDT.IssueQuantity, 0)) AS TotalIssuedQty
                        FROM Inventory.IssueHeader AS IHD
                        INNER JOIN Inventory.IssueDetail AS IDT 
                            ON IHD.Id = IDT.IssueHeaderId
                        GROUP BY IHD.MrsHeaderId, IDT.ItemId
                    ),
                    FilteredMrs AS
                    (
                        SELECT 
                            A.Id AS MrsId,
                            A.UnitId,
                            A.RequestCategoryId,
                            CD.Description AS RequestCategoryName,
                            A.MrsNo,
                            A.MrsDate,
                            A.DepartmentId,
                            A.SubDepartmentId,
                            A.Remarks,
                            A.SubStoresWarehouseId,
                            A.StatusId,
                            C.Description AS StatusName,
                            SUM(B.RequestQuantity) AS RequestedQuantity,
                            SUM(ISNULL(ICTE.TotalIssuedQty, 0)) AS IssuedQuantity,
                            SUM(B.RequestQuantity) - SUM(ISNULL(ICTE.TotalIssuedQty, 0)) AS PendingQuantity,
                            CASE 
                                WHEN SUM(ISNULL(ICTE.TotalIssuedQty, 0)) = 0 THEN 'N'
                                WHEN SUM(B.RequestQuantity) - SUM(ISNULL(ICTE.TotalIssuedQty, 0)) = 0 THEN 'Y'
                                ELSE 'P'
                            END AS IssueStatus
                        FROM Inventory.MrsHeader AS A
                        INNER JOIN Inventory.MrsDetail AS B 
                            ON A.Id = B.MrsHeaderId
                        LEFT JOIN IssuedCTE AS ICTE 
                            ON A.Id = ICTE.MrsHeaderId 
                            AND B.ItemId = ICTE.ItemId
                        INNER JOIN Inventory.MiscMaster AS C 
                            ON A.StatusId = C.Id
                        INNER JOIN Inventory.MiscMaster AS CD
                            ON A.RequestCategoryId = CD.Id
                        {whereClause}
                        GROUP BY 
                            A.Id, A.UnitId, A.RequestCategoryId, A.MrsNo, A.MrsDate,
                            A.DepartmentId, A.SubDepartmentId, A.Remarks,
                            A.SubStoresWarehouseId, A.StatusId, C.Description, CD.Description
                    )
                    SELECT *
                    FROM FilteredMrs
                    ORDER BY MrsDate DESC
                    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                    -- Count query
                    WITH IssuedCTE AS
                    (
                        SELECT 
                            IHD.MrsHeaderId,
                            IDT.ItemId,
                            SUM(ISNULL(IDT.IssueQuantity, 0)) AS TotalIssuedQty
                        FROM Inventory.IssueHeader AS IHD
                        INNER JOIN Inventory.IssueDetail AS IDT 
                            ON IHD.Id = IDT.IssueHeaderId
                        GROUP BY IHD.MrsHeaderId, IDT.ItemId
                    ),
                    FilteredMrs AS
                    (
                        SELECT 
                            A.Id
                        FROM Inventory.MrsHeader AS A
                        INNER JOIN Inventory.MrsDetail AS B 
                            ON A.Id = B.MrsHeaderId
                        LEFT JOIN IssuedCTE AS ICTE 
                            ON A.Id = ICTE.MrsHeaderId 
                            AND B.ItemId = ICTE.ItemId
                        INNER JOIN Inventory.MiscMaster AS C 
                            ON A.StatusId = C.Id
                        {whereClause}
                        GROUP BY A.Id, A.UnitId
                    )
                    SELECT COUNT(1) FROM FilteredMrs;
                    ";

            using (var multi = await _dbConnection.QueryMultipleAsync(query, parameters))
            {
                var data = (await multi.ReadAsync<GetPendingIssueHeaderDto>()).ToList();
                var totalCount = await multi.ReadFirstAsync<int>();
                return (data, totalCount);
            }
        }

        public async Task<List<GetPendingIssueDto>> GetPendingIssuesAsync(int mrsNo)
        {
            var UnitId = _ipAddressService.GetUnitId();

    var query = @"
        SELECT 
            A.Id AS MrsId,
            A.UnitId,
            A.RequestCategoryId,
            RC.Description AS RequestCategoryName,
            A.MrsNo,
            A.MrsDate,
            A.DepartmentId,
            A.SubDepartmentId,
            A.Remarks,
            A.SubStoresWarehouseId,
            A.StatusId,
            C.Description AS StatusName,

            B.Id AS MrsDetailId,
            B.MrsHeaderId,
            B.ItemId,
            IM.ItemCode AS ItemCode,
            IM.ItemName AS ItemName,
            B.UomId,
            US.UOMName AS UomName,
            B.RequestQuantity,
            B.CostCenterId,
            B.FinanceCode,
            B.WarehouseStockId,

            ISNULL(SUM(IDT.IssueQuantity), 0) AS IssuedQuantity,
            (B.RequestQuantity - ISNULL(SUM(IDT.IssueQuantity), 0)) AS PendingQuantity

        FROM Inventory.MrsHeader AS A
        INNER JOIN Inventory.MrsDetail AS B 
            ON A.Id = B.MrsHeaderId
        LEFT JOIN Inventory.IssueHeader AS IHD 
            ON A.Id = IHD.MrsHeaderId AND IHD.UnitId = A.UnitId
        LEFT JOIN Inventory.IssueDetail AS IDT 
            ON IHD.Id = IDT.IssueHeaderId 
            AND IDT.ItemId = B.ItemId 
            AND B.Id = IDT.Sno
        INNER JOIN Inventory.MiscMaster AS C 
            ON A.StatusId = C.Id
        INNER JOIN Inventory.MiscMaster AS RC ON A.RequestCategoryId = RC.Id
        INNER JOIN Inventory.ItemMaster AS IM ON B.ItemId = IM.Id
        INNER JOIN Inventory.UOM AS US ON B.UomId = US.Id

        WHERE 
            (@MrsId IS NULL OR A.Id = @MrsId)
            AND A.UnitId = @UnitId
            AND C.Description = @Status   -- ✅ Only Approved MRS

        GROUP BY 
            A.Id, A.UnitId, A.RequestCategoryId, A.MrsNo, A.MrsDate,
            A.DepartmentId, A.SubDepartmentId, A.Remarks,
            A.SubStoresWarehouseId, A.StatusId,
            B.Id, B.MrsHeaderId, B.ItemId, B.UomId,
            B.RequestQuantity, B.CostCenterId, B.FinanceCode, B.WarehouseStockId,RC.Description,C.Description,IM.ItemCode,IM.ItemName,US.UOMName

      

        ORDER BY 
            A.MrsNo;";

    var lookup = new Dictionary<int, GetPendingIssueDto>();

    var result = await _dbConnection.QueryAsync<
        GetPendingIssueDto,
        GetPendingIssueDto.GetPendingIssueDetailsDto,
        GetPendingIssueDto>(
        query,
        (header, detail) =>
        {
            if (!lookup.TryGetValue(header.MrsId, out var headerEntry))
            {
                headerEntry = header;
                headerEntry.PendingIssueDetails = new List<GetPendingIssueDto.GetPendingIssueDetailsDto>();
                lookup.Add(header.MrsId, headerEntry);
            }

            headerEntry.PendingIssueDetails.Add(detail);
            return headerEntry;
        },
        new 
        { 
            MrsId = mrsNo, 
            UnitId = UnitId,
            Status = MiscEnumEntity.Approved  // ✅ Using constant from MiscEnumEntity
        },
        splitOn: "MrsDetailId"
    );

    return lookup.Values.ToList();
        }
    }
}