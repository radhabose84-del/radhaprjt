#nullable disable
using System.Data;
using WarehouseManagement.Application.BinMaster.Queries.GetAllBinMaster;
using WarehouseManagement.Application.BinMaster.Queries.GetBinMasterAutoComplete;
using WarehouseManagement.Application.Common.Interfaces.IBinMaster;
using Dapper;

namespace WarehouseManagement.Infrastructure.Repositories.BinMaster
{
    public class BinMasterQueryRepository : IBinMasterQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        public BinMasterQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<(List<BinMasterDto>, int)> GetAllAsync(int pageNumber, int pageSize, string searchTerm)
        {
            var sql = @"
                DECLARE @Search NVARCHAR(200) = @SearchParam;
                DECLARE @TotalCount INT;

                -- Total count (same filter as page)
                SELECT @TotalCount = COUNT(*)
                FROM Warehouse.BinMaster b
                LEFT JOIN Warehouse.WarehouseMaster w ON w.Id = b.WarehouseId
                LEFT JOIN Warehouse.RackMaster r      ON r.Id = b.RackId
                WHERE b.IsDeleted = 0
                AND (
                        @Search IS NULL OR @Search = '' OR
                        b.BinCode LIKE @Search OR
                        b.BinName LIKE @Search OR
                        w.WarehouseCode LIKE @Search OR
                        w.WarehouseName LIKE @Search OR
                        r.RackCode LIKE @Search OR
                        r.RackName LIKE @Search
                    );

                -- Page Ids into temp table
                IF OBJECT_ID('tempdb..#PagedIds') IS NOT NULL DROP TABLE #PagedIds;
                CREATE TABLE #PagedIds (Id INT PRIMARY KEY);

                INSERT INTO #PagedIds(Id)
                SELECT b.Id
                FROM Warehouse.BinMaster b
                LEFT JOIN Warehouse.WarehouseMaster w ON w.Id = b.WarehouseId
                LEFT JOIN Warehouse.RackMaster r      ON r.Id = b.RackId
                WHERE b.IsDeleted = 0
                AND (
                        @Search IS NULL OR @Search = '' OR
                        b.BinCode LIKE @Search OR
                        b.BinName LIKE @Search OR
                        w.WarehouseCode LIKE @Search OR
                        w.WarehouseName LIKE @Search OR
                        r.RackCode LIKE @Search OR
                        r.RackName LIKE @Search
                    )
                ORDER BY b.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                -- Page details
                SELECT 
                    b.Id,
                    b.WarehouseId,
                    b.RackId,
                    b.BinCode,
                    b.BinName,
                    b.BinCapacity,
                    b.CapacityUOMId,
                    b.IsActive,
                    b.IsDeleted,
                    b.CreatedBy,
                    b.CreatedDate,
                    b.CreatedByName,
                    b.CreatedIP,
                    b.ModifiedBy,
                    b.ModifiedDate,
                    b.ModifiedByName,
                    b.ModifiedIP,
                    -- convenience name fields (for display)
                    w.WarehouseCode   AS WarehouseCode,
                    w.WarehouseName   AS WarehouseName,
                    r.RackCode        AS RackCode,
                    r.RackName        AS RackName
                FROM Warehouse.BinMaster b
                JOIN #PagedIds p ON p.Id = b.Id
                LEFT JOIN Warehouse.WarehouseMaster w ON w.Id = b.WarehouseId
                LEFT JOIN Warehouse.RackMaster r      ON r.Id = b.RackId
                ORDER BY b.Id DESC;

                -- TotalCount
                SELECT @TotalCount AS TotalCount;
            ";

            var parameters = new
            {
                SearchParam = string.IsNullOrWhiteSpace(searchTerm) ? null : $"%{searchTerm}%",
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            using var multi = await _dbConnection.QueryMultipleAsync(sql, parameters);

            var bins = (await multi.ReadAsync<BinMasterDto>()).ToList();
            var total = await multi.ReadFirstAsync<int>();

            return (bins, total);
        }


        public async Task<BinMasterDto> GetByIdAsync(int id)
        {
            const string sql = @"
                    SELECT 
                        b.Id,
                        b.BinCode,
                        b.BinName,
                        b.WarehouseId,
                        w.WarehouseName, 
                        b.BinCapacity,                                      
                        b.CapacityUOMId,
						b.Rackid,
						r.RackName,
                        b.IsActive,RackCode
                    FROM Warehouse.BinMaster b WITH (NOLOCK)
                    INNER JOIN Warehouse.WarehouseMaster w ON b.WarehouseId = w.Id 
					LEFT JOIN Warehouse.RackMaster r on b.RackId=r.id                   
                    WHERE b.IsDeleted = 0 AND b.Id = @Id;";

            return await _dbConnection.QueryFirstOrDefaultAsync<BinMasterDto>(sql, new { Id = id });
        }


        public async Task<bool> ExistsByWarehouseAndCodeAsync(int warehouseId, string binCode, CancellationToken ct = default)
        {
            const string sql = @"
                    SELECT 1
                    FROM Warehouse.BinMaster WITH (NOLOCK)
                    WHERE IsDeleted = 0
                    AND WarehouseId = @WarehouseId
                    AND BinCode = @BinCode";

            var exists = await _dbConnection.ExecuteScalarAsync<int?>(new CommandDefinition(
                sql, new { WarehouseId = warehouseId, BinCode = binCode }, cancellationToken: ct));

            return exists.HasValue;
        }

        public async Task<IEnumerable<string>> GetBinCodesByPrefixAsync(string prefix, CancellationToken ct = default)
        {
            const string sql = @"
                SELECT BinCode
                FROM Warehouse.BinMaster WITH (NOLOCK)
                WHERE 
                 BinCode LIKE @Prefix + '%';
            ";

            var cmd = new CommandDefinition(sql, new { Prefix = prefix }, cancellationToken: ct);
            return await _dbConnection.QueryAsync<string>(cmd);
        }

        public async Task<IReadOnlyList<BinAutoDto>> AutocompleteAsync(string q, int top = 10, int? warehouseId = null, int? rackId = null, CancellationToken ct = default)
        
            {
                const string sql = @"
                    SELECT 
                        b.Id,
                        b.BinCode,
                        b.BinName
                    FROM Warehouse.BinMaster b WITH (NOLOCK)
                    WHERE b.IsDeleted = 0
                    AND b.IsActive = 1
                    AND (@WarehouseId IS NULL OR @WarehouseId = 0 OR b.WarehouseId = @WarehouseId)
                    AND (@RackId      IS NULL OR b.RackId      = @RackId)
                    AND (
                            @Q IS NULL OR @Q = '' 
                            OR b.BinCode LIKE @Like OR b.BinName LIKE @Like
                        )
                    ORDER BY
                        CASE WHEN b.BinCode = @Q THEN 0
                            WHEN b.BinName = @Q THEN 1
                            ELSE 2 END,
                        b.BinCode;";

                var like = string.IsNullOrWhiteSpace(q) ? null : $"{q.Trim()}%";

                var param = new
                {
                    Top = top <= 0 ? 10 : top,
                    WarehouseId = warehouseId,
                    RackId = rackId,
                    Q = q,
                    Like = like
                };

                var cmd = new CommandDefinition(sql, param, cancellationToken: ct);
                var rows = await _dbConnection.QueryAsync<BinAutoDto>(cmd);
                return rows.AsList();

            }
        
    }
}
