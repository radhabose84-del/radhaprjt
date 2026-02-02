using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IIssue;
using PurchaseManagement.Application.Issue.Queries.GetApprovedMrsById;
using PurchaseManagement.Application.Issue.Queries.GetPendingIssue;
using PurchaseManagement.Application.Issue.Queries.GetPendingIssueHeader;
using PurchaseManagement.Application.IssueReturn.Queries.GetPendingIssueReturn;
using PurchaseManagement.Application.IssueReturn.Queries.GetPendingIssueReturnById;
using PurchaseManagement.Domain.Common;
using Dapper;
using static PurchaseManagement.Application.Issue.Queries.GetPendingIssue.GetPendingIssueDto;

namespace PurchaseManagement.Infrastructure.Repositories.Issue
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
                FROM Purchase.MrsHeader a
                INNER JOIN Purchase.MiscMaster mm 
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

      public async Task<PendingIssueReturnByIdDto> GetByIdAsync(int id)
    {
        var UnitId = _ipAddressService.GetUnitId();

        const string query = @"
            SELECT 
                A.Id as IssueReturnId,
                A.UnitId,
                A.IssueReturnNo,
                A.IssueReturnDate,
                A.IssueHeaderId,
                A.RequestCategoryId,
                B.Description AS RequestCategoryName,
                A.DepartmentId,
                A.Remarks,
                A.CreatedBy,
                A.CreatedByName,
                A.CreatedDate,
                D.Id,
                D.IssueReturnHeaderId,
                D.ItemId,
                D.UomId,
                D.WarehouseStockId,
                D.StorageTypeId,
                D.TargetId,
                D.ReturnQuantity,
                D.ReturnValue,
                D.ReasonId,
                E.Description AS ReasonName,
                D.SubStoresDepartmentId
            FROM Purchase.IssueReturnHeader A
            INNER JOIN Purchase.IssueReturnDetail D ON A.Id = D.IssueReturnHeaderId
            INNER JOIN Purchase.MiscMaster B ON A.RequestCategoryId = B.Id 
            INNER JOIN Purchase.MiscMaster C ON A.StatusId = C.Id 
            INNER JOIN Purchase.MiscMaster E ON D.ReasonId = E.Id
            WHERE 
                A.UnitId = @UnitId
                AND A.Id = @Id
                AND C.Description = @Pending;
        ";

        var lookup = new Dictionary<int, PendingIssueReturnByIdDto>();

        var result = await _dbConnection.QueryAsync<PendingIssueReturnByIdDto, PendingIssueReturnDetailsByIdDto, PendingIssueReturnByIdDto>(
            query,
            (header, detail) =>
            {
                if (!lookup.TryGetValue(header.IssueReturnId, out var headerEntry))
                {
                    headerEntry = header;
                    headerEntry.PendingIssueReturnDetails = new List<PendingIssueReturnDetailsByIdDto>();
                    lookup.Add(headerEntry.IssueReturnId, headerEntry);
                }

                headerEntry.PendingIssueReturnDetails.Add(detail);
                return headerEntry;
            },
            new { UnitId, Id = id, Pending = MiscEnumEntity.Pending }, // or MiscEnumEntity.Pending
            splitOn: "Id"
        );

        return lookup.Values.FirstOrDefault();
    }


        public async Task<string?> GetDescriptionByIdAsync(int id)
        {
             var sql = "SELECT Description FROM Purchase.MiscMaster WHERE Id = @Id";
             return await _dbConnection.QueryFirstOrDefaultAsync<string?>(sql, new { Id = id });
        }

       public async Task<List<GetPendingStockBinDto>> GetMainStoresStockBinWise(List<int>? itemIds, int warehouseId)
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
                FROM Purchase.StockLedger
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

        public async Task<(List<GetPendingIssueHeaderDto>, int)> GetPendingIssueHeaderAsync(
            DateTimeOffset? fromDate,
            DateTimeOffset? toDate,
            int PageNumber,
            int PageSize,
            string? SearchTerm)
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
                        FROM Purchase.IssueHeader AS IHD
                        INNER JOIN Purchase.IssueDetail AS IDT 
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
                        FROM Purchase.MrsHeader AS A
                        INNER JOIN Purchase.MrsDetail AS B 
                            ON A.Id = B.MrsHeaderId
                        LEFT JOIN IssuedCTE AS ICTE 
                            ON A.Id = ICTE.MrsHeaderId 
                            AND B.ItemId = ICTE.ItemId
                        INNER JOIN Purchase.MiscMaster AS C 
                            ON A.StatusId = C.Id
                        INNER JOIN Purchase.MiscMaster AS CD
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
                        FROM Purchase.IssueHeader AS IHD
                        INNER JOIN Purchase.IssueDetail AS IDT 
                            ON IHD.Id = IDT.IssueHeaderId
                        GROUP BY IHD.MrsHeaderId, IDT.ItemId
                    ),
                    FilteredMrs AS
                    (
                        SELECT 
                            A.Id
                        FROM Purchase.MrsHeader AS A
                        INNER JOIN Purchase.MrsDetail AS B 
                            ON A.Id = B.MrsHeaderId
                        LEFT JOIN IssuedCTE AS ICTE 
                            ON A.Id = ICTE.MrsHeaderId 
                            AND B.ItemId = ICTE.ItemId
                        INNER JOIN Purchase.MiscMaster AS C 
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

        public async Task<(List<PendingIssueReturnDto>, int)> GetPendingIssueReturnAsync(int PageNumber, int PageSize, string? SearchTerm)
        {
            var UnitId = _ipAddressService.GetUnitId();

            const string dataQuery = @"
                SELECT 
                    A.Id,
                    A.IssueReturnNo,
                    A.IssueReturnDate,
                    A.RequestCategoryId,
                    ReqCat.Description AS RequestCategoryName,
                    A.UnitId,
                    A.IssueHeaderId,
                    A.DepartmentId,
                    A.Remarks,
                    A.CreatedBy,
                    A.CreatedByName,
                    A.CreatedDate
                FROM Purchase.IssueReturnHeader A
                INNER JOIN Purchase.MiscMaster ReqCat ON A.RequestCategoryId = ReqCat.Id
                INNER JOIN Purchase.MiscMaster Stat ON A.StatusId = Stat.Id
                WHERE 
                     Stat.Description = @Pending
                    AND A.UnitId = @UnitId
                    AND (@Search IS NULL 
                        OR A.IssueReturnNo LIKE @Search 
                        OR ReqCat.Description LIKE @Search 
                        OR A.CreatedByName LIKE @Search)
                ORDER BY A.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            ";

            const string countQuery = @"
                SELECT COUNT(1)
                FROM Purchase.IssueReturnHeader A
                INNER JOIN Purchase.MiscMaster ReqCat ON A.RequestCategoryId = ReqCat.Id
                INNER JOIN Purchase.MiscMaster Stat ON A.StatusId = Stat.Id
                WHERE 
                       Stat.Description = @Pending
                    AND A.UnitId = @UnitId
                    AND (@Search IS NULL 
                        OR A.IssueReturnNo LIKE @Search 
                        OR ReqCat.Description LIKE @Search 
                        OR A.CreatedByName LIKE @Search);
            ";

            var parameters = new
            {
                Search = string.IsNullOrEmpty(SearchTerm) ? null : $"%{SearchTerm}%",
                Offset = (PageNumber - 1) * PageSize,
                PageSize,
                UnitId,
                Pending = MiscEnumEntity.Pending // or MiscEnumEntity.Pending if that resolves to the same string
            };

            // Fetch paginated list
            var issueReturns = await _dbConnection.QueryAsync<PendingIssueReturnDto>(dataQuery, parameters);

            // Fetch total count
            var totalCount = await _dbConnection.ExecuteScalarAsync<int>(countQuery, parameters);

            return (issueReturns.ToList(), totalCount);
        }


        public async Task<List<GetPendingIssueDto>> GetPendingIssuesAsync(int mrsid)
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
            B.UomId,
            B.RequestQuantity,
            B.CostCenterId,
            B.FinanceCode,
            B.WarehouseStockId,

            ISNULL(SUM(IDT.IssueQuantity), 0) AS IssuedQuantity,
            (B.RequestQuantity - ISNULL(SUM(IDT.IssueQuantity), 0)) AS PendingQuantity

        FROM Purchase.MrsHeader AS A
        INNER JOIN Purchase.MrsDetail AS B 
            ON A.Id = B.MrsHeaderId
        LEFT JOIN Purchase.IssueHeader AS IHD 
            ON A.Id = IHD.MrsHeaderId AND IHD.UnitId = A.UnitId
        LEFT JOIN Purchase.IssueDetail AS IDT 
            ON IHD.Id = IDT.IssueHeaderId 
            AND IDT.ItemId = B.ItemId 
            AND B.Id = IDT.Sno
        INNER JOIN Purchase.MiscMaster AS C 
            ON A.StatusId = C.Id
        INNER JOIN Purchase.MiscMaster AS RC ON A.RequestCategoryId = RC.Id

        WHERE 
            (@MrsId IS NULL OR A.Id = @MrsId)
            AND A.UnitId = @UnitId
            AND C.Description = @Status   -- ✅ Only Approved MRS

        GROUP BY 
            A.Id, A.UnitId, A.RequestCategoryId, A.MrsNo, A.MrsDate,
            A.DepartmentId, A.SubDepartmentId, A.Remarks,
            A.SubStoresWarehouseId, A.StatusId,
            B.Id, B.MrsHeaderId, B.ItemId, B.UomId,
            B.RequestQuantity, B.CostCenterId, B.FinanceCode, B.WarehouseStockId,RC.Description,C.Description

      

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
            MrsId = mrsid, 
            UnitId = UnitId,
            Status = MiscEnumEntity.Approved  // ✅ Using constant from MiscEnumEntity
        },
        splitOn: "MrsDetailId"
    );

    return lookup.Values.ToList();
}

    }
}