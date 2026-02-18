#nullable disable
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IMRS;
using PurchaseManagement.Application.MRS.Queries.GetMrsEntry;
using PurchaseManagement.Application.MRS.Queries.GetMrsEntryById;
using PurchaseManagement.Application.MRS.Queries.GetMrsPending;
using PurchaseManagement.Application.MRS.Queries.GetStockItemBased;
using PurchaseManagement.Domain.Common;
using Dapper;
using static PurchaseManagement.Application.MRS.Queries.GetMrsEntry.GetMrsEntryDto;

namespace PurchaseManagement.Infrastructure.Repositories.MRS
{
    public class MrsEntryQueryRepository : IMrsEntryQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IIPAddressService _ipAddressService;
        public MrsEntryQueryRepository(IDbConnection dbConnection, IIPAddressService ipAddressService)
        {
            _dbConnection = dbConnection;
            _ipAddressService = ipAddressService;
        }

        public async Task<GetMrsEntryByIdDto> GetMrsDetailsById(int id)
        {
             var UnitId = _ipAddressService.GetUnitId();
            var sql = @"
            SELECT 
            A.Id,
            A.MrsNo,
            A.MrsDate,
            A.UnitId,
            A.RequestCategoryId,
            C.description AS RequestCategoryName,
            A.StatusId,
            D.description AS StatusName,
            A.DepartmentId,
            A.SubDepartmentId,
            A.Remarks,
            A.CreatedBy,
            A.CreatedByName,
            A.CreatedDate,
            A.ModifiedBy,
            A.ModifiedByName,
            A.ModifiedDate,
            A.ApprovedBy,
            A.ApprovedByName,
            A.ApprovedDate,
            A.ApprovedIP,
            A.SubStoresWarehouseId
        FROM Purchase.MrsHeader A
        INNER JOIN Purchase.MiscMaster C ON A.RequestCategoryId = C.Id
        INNER JOIN Purchase.MiscMaster D ON A.StatusId = D.Id
        WHERE A.Id = @Id AND A.UnitId = @UnitId;
        SELECT * FROM Purchase.MrsDetail WHERE MrsHeaderId = @Id;
";

            using var multi = await _dbConnection.QueryMultipleAsync(sql, new { Id = id, UnitId = UnitId });
            var getMrsEntryBy = await multi.ReadFirstOrDefaultAsync<GetMrsEntryByIdDto>();

            if (getMrsEntryBy is null)
                return null; // Let handler handle NotFound

            getMrsEntryBy.MrsDetails = (await multi.ReadAsync<GetMrsEntryByIdDto.GetMrsDetailDtoById>()).ToList();
            return getMrsEntryBy;
        }

    public async Task<(List<GetMrsEntryDto>, int)> GetMrsEntryDetails(
    int PageNumber,
    int PageSize,
    string SearchTerm,
    DateTimeOffset? fromDate,
    DateTimeOffset? toDate)
{
    var UnitId = _ipAddressService.GetUnitId();

    var parameters = new DynamicParameters();
    parameters.Add("UnitId", UnitId);

    // WHERE clause
    var whereClause = "WHERE A.UnitId = @UnitId";

    if (fromDate.HasValue)
    {
        whereClause += " AND A.CreatedDate >= @FromDate";
        parameters.Add("FromDate", fromDate.Value);
    }

    if (toDate.HasValue)
    {
        var toDateEnd = toDate.Value.Date.AddDays(1).AddTicks(-1); // End of day
        whereClause += " AND A.CreatedDate <= @ToDate";
        parameters.Add("ToDate", toDateEnd);
    }

    if (!string.IsNullOrWhiteSpace(SearchTerm))
    {
        whereClause += @" AND (
            A.MrsNo LIKE @SearchTerm OR
            A.Id LIKE @SearchTerm OR
            C.Name LIKE @SearchTerm
        )";
        parameters.Add("SearchTerm", $"%{SearchTerm}%");
    }

    // Pagination
    var offset = (PageNumber - 1) * PageSize;
    parameters.Add("Offset", offset);
    parameters.Add("PageSize", PageSize);

    // Main query with pagination
    var query = $@"
        SELECT 
            A.Id AS Id,
            A.MrsNo,
            A.MrsDate,
            A.UnitId,
            A.RequestCategoryId,
            C.description AS RequestCategoryName,
            A.StatusId,
            D.description AS StatusName,
            A.DepartmentId,
            A.SubDepartmentId,
            A.Remarks,
            A.CreatedBy,
            A.CreatedByName,
            A.CreatedDate,
            A.ModifiedBy,
            A.ModifiedByName,
            A.ModifiedDate,
            A.ApprovedByName,
            A.ApprovedDate,
            A.SubStoresWarehouseId
        FROM Purchase.MrsHeader A
        INNER JOIN Purchase.MiscMaster C ON A.RequestCategoryId = C.Id
        INNER JOIN Purchase.MiscMaster D ON A.StatusId = D.Id
        {whereClause}
        ORDER BY A.CreatedDate DESC
        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

        SELECT COUNT(1)
        FROM Purchase.MrsHeader A
        INNER JOIN Purchase.MiscMaster C ON A.RequestCategoryId = C.Id
        INNER JOIN Purchase.MiscMaster D ON A.StatusId = D.Id
        {whereClause};
    ";

    using (var multi = await _dbConnection.QueryMultipleAsync(query, parameters))
    {
        var headerList = (await multi.ReadAsync<GetMrsEntryDto>()).ToList();
        var totalCount = await multi.ReadFirstAsync<int>();

        // Fetch Details for the returned headers
        if (headerList.Any())
        {
            var headerIds = headerList.Select(x => x.Id).ToList();
            var detailQuery = @"
                SELECT 
                    B.Id,
                    B.MrsHeaderId,
                    B.ItemId,
                    B.UomId,
                    B.RequestQuantity,
                    B.CostCenterId,
                    B.FinanceCode,
                    B.WarehouseStockId
                FROM Purchase.MrsDetail B
                WHERE B.MrsHeaderId IN @HeaderIds;
            ";

            var detailData = await _dbConnection.QueryAsync<GetMrsDetailDto>(
                detailQuery,
                new { HeaderIds = headerIds }
            );

            var groupedDetails = detailData.GroupBy(d => d.MrsHeaderId).ToDictionary(g => g.Key, g => g.ToList());

            foreach (var header in headerList)
            {
                if (groupedDetails.TryGetValue(header.Id, out var details))
                    header.MrsDetails = details;
            }
        }

        return (headerList, totalCount);
    }
}

       public async Task<(List<MrsPendingDto>, int)> GetPendingMrsDetailsAsync(
            int PageNumber, 
            int PageSize, 
            string SearchTerm)
        {
            var UnitId = _ipAddressService.GetUnitId();

            const string dataQuery = @"
                SELECT 
                    A.Id AS Id,
                    A.MrsNo,
                    A.MrsDate,
                    A.UnitId,
                    A.RequestCategoryId,
                    RC.Description AS RequestCategoryName,
                    A.StatusId,
                    C.Description AS StatusName,
                    A.DepartmentId,
                    A.SubDepartmentId,
                    A.Remarks,
                    A.CreatedBy,
                    A.CreatedByName,
                    A.CreatedDate,
                    A.ModifiedBy,
                    A.ModifiedByName,
                    A.ModifiedDate,

                    -- Detail section
                    B.Id AS MrsDetailId,
                    B.MrsHeaderId,
                    B.ItemId,
                    B.UomId,
                    B.RequestQuantity,
                    B.CostCenterId,
                    B.FinanceCode

                FROM Purchase.MrsHeader AS A
                INNER JOIN Purchase.MrsDetail AS B ON A.Id = B.MrsHeaderId
                INNER JOIN Purchase.MiscMaster AS C ON A.StatusId = C.Id
                INNER JOIN Purchase.MiscMaster AS RC ON A.RequestCategoryId = RC.Id

                WHERE 
                    A.UnitId = @UnitId
                    AND C.Description = @Status
                    AND (@Search IS NULL 
                        OR A.MrsNo LIKE @Search 
                        OR A.DepartmentId LIKE @Search 
                        OR A.SubDepartmentId LIKE @Search)

                ORDER BY A.MrsDate DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            ";

            const string countQuery = @"
                SELECT COUNT(DISTINCT A.Id)
                FROM Purchase.MrsHeader AS A
                INNER JOIN Purchase.MrsDetail AS B ON A.Id = B.MrsHeaderId
                INNER JOIN Purchase.MiscMaster AS C ON A.StatusId = C.Id
                WHERE 
                    A.UnitId = @UnitId
                    AND C.Description = @Status
                    AND (@Search IS NULL 
                        OR A.MrsNo LIKE @Search);
            ";

            var parameters = new
            {
                UnitId,
                Offset = (PageNumber - 1) * PageSize,
                PageSize,
                Search = string.IsNullOrWhiteSpace(SearchTerm) ? null : $"%{SearchTerm}%",
                Status = MiscEnumEntity.Pending
            };

            var lookup = new Dictionary<int, MrsPendingDto>();

            var result = await _dbConnection.QueryAsync<
                MrsPendingDto,
                MrsPendingDto.GetMrsPendingDetailDto,
                MrsPendingDto>(
                dataQuery,
                (header, detail) =>
                {
                    if (!lookup.TryGetValue(header.Id, out var headerEntry))
                    {
                        headerEntry = header;
                        headerEntry.MrsDetails = new List<MrsPendingDto.GetMrsPendingDetailDto>();
                        lookup.Add(header.Id, headerEntry);
                    }

                    headerEntry.MrsDetails.Add(detail);
                    return headerEntry;
                },
                parameters,
                splitOn: "MrsDetailId"
            );

            var totalCount = await _dbConnection.ExecuteScalarAsync<int>(countQuery, parameters);

            return (lookup.Values.ToList(), totalCount);
        }
        public async Task<List<GetStockItemDto>> GetStockDetails(int itemId, int warehouseId)
        {
            var UnitId = _ipAddressService.GetUnitId();
            string sql = @"
                SELECT 
                    ItemId,
                    WarehouseId,
                    UomId,
                    SUM(ReceivedQty - IssueQty) AS CurrentStockQty,
                    SUM(ReceivedValue - IssueValue) AS CurrentStockValue
                FROM Purchase.StockLedger
                WHERE UnitId = @UnitId 
                  AND (@WarehouseId IS NULL OR WarehouseId = @WarehouseId)
                  AND (@ItemId IS NULL OR ItemId = @ItemId)
                GROUP BY UnitId, ItemId, WarehouseId, UomId
                HAVING SUM(ReceivedQty - IssueQty) <> 0;
            ";

            var result = await _dbConnection.QueryAsync<GetStockItemDto>(sql, new
            {
                UnitId = UnitId,
                WarehouseId = warehouseId,
                ItemId = itemId
            });

            return result.ToList();
        }
    }
}