using System.Data;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetAmc;
using FAM.Domain.Common;
using FAM.Domain.Entities.AssetMaster;
using Dapper;
using Serilog;

namespace FAM.Infrastructure.Repositories.AssetMaster.AssetAmc
{
    public class AssetAmcQueryRepository : IAssetAmcQueryRepository
    {
        private readonly IDbConnection _dbConnection; 
        public AssetAmcQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<(List<FAM.Domain.Entities.AssetMaster.AssetAmc>, int)> GetAllAssetAmcAsync(int PageNumber, int PageSize, string? SearchTerm)
        {
            var query = $$"""
             DECLARE @TotalCount INT;
             SELECT @TotalCount = COUNT(*) 
               FROM FixedAsset.AssetAmc
              WHERE IsDeleted = 0
            {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (AssetId LIKE @Search OR VendorName LIKE @Search)")}};

                SELECT 
                Id, 
                AssetId,
                StartDate,
                EndDate,
                Period,
                VendorCode,
                VendorName,
                VendorPhone,
                VendorEmail,
                CoverageType,
                FreeServiceCount,
                RenewalStatus,
                RenewedDate,
                IsDeleted,
                IsActive
            FROM FixedAsset.AssetAmc 
            WHERE 
            IsDeleted = 0
                {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (AssetId LIKE @Search OR VendorName LIKE @Search )")}}
                ORDER BY Id desc
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;
            """;

            
             var parameters = new
                       {
                           Search = $"%{SearchTerm}%",
                           Offset = (PageNumber - 1) * PageSize,
                           PageSize
                       };

             var assetamc = await _dbConnection.QueryMultipleAsync(query, parameters);
             var assetamclist = (await assetamc.ReadAsync<FAM.Domain.Entities.AssetMaster.AssetAmc>()).ToList();
             int totalCount = (await assetamc.ReadFirstAsync<int>());
             return (assetamclist, totalCount);
        }

        public async Task<FAM.Domain.Entities.AssetMaster.AssetAmc?> GetByIdAsync(int Id)
        {
             const string query = @"
                    SELECT * 
                    FROM FixedAsset.AssetAmc 
                    WHERE Id = @Id AND IsDeleted = 0";

                    var AssetAmc = await _dbConnection.QueryFirstOrDefaultAsync<FAM.Domain.Entities.AssetMaster.AssetAmc>(query, new { Id });
                    return AssetAmc;
        }

        public async Task<List<FAM.Domain.Entities.MiscMaster>> GetCoverageScopeAsync()
        {
            const string query = @"
            SELECT M.Id,MiscTypeId,Code,M.Description,SortOrder,  M.IsActive
            ,M.CreatedBy,M.CreatedDate,M.CreatedByName,M.CreatedIP,M.ModifiedBy,M.ModifiedDate,M.ModifiedByName,M.ModifiedIP
            FROM FixedAsset.MiscMaster M
            INNER JOIN FixedAsset.MiscTypeMaster T on T.ID=M.MiscTypeId
            WHERE (MiscTypeCode = @MiscTypeCode) 
            AND  M.IsDeleted=0 and M.IsActive=1
            ORDER BY M.ID DESC";    
            var parameters = new { MiscTypeCode = MiscEnumEntity.Asset_AmcCoverageScope.MiscCode };        
            var result = await _dbConnection.QueryAsync<FAM.Domain.Entities.MiscMaster>(query,parameters);
            return result.ToList();
        }

        public async Task<List<FAM.Domain.Entities.MiscMaster>> GetRenewStatusAsync()
        {
            const string query = @"
            SELECT M.Id,MiscTypeId,Code,M.Description,SortOrder,  M.IsActive
            ,M.CreatedBy,M.CreatedDate,M.CreatedByName,M.CreatedIP,M.ModifiedBy,M.ModifiedDate,M.ModifiedByName,M.ModifiedIP
            FROM FixedAsset.MiscMaster M
            INNER JOIN FixedAsset.MiscTypeMaster T on T.ID=M.MiscTypeId
            WHERE (MiscTypeCode = @MiscTypeCode) 
            AND  M.IsDeleted=0 and M.IsActive=1
            ORDER BY M.ID DESC";    
            var parameters = new { MiscTypeCode = MiscEnumEntity.Asset_AmcRenewStatus.MiscCode };        
            var result = await _dbConnection.QueryAsync<FAM.Domain.Entities.MiscMaster>(query,parameters);
            return result.ToList();
        }

        public async Task<List<ExistingVendorDetails>> GetVendorDetails(string OldUnitId, string? VendorCode)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@OldUnitId", OldUnitId, DbType.String);

            // Correctly pass NULL or wildcard pattern
            if (!string.IsNullOrWhiteSpace(VendorCode))
            {
                parameters.Add("@Slcode", VendorCode, DbType.String); // No wildcard in C# - handled in SQL
            }
            else
            {
                parameters.Add("@Slcode", DBNull.Value, DbType.String);
            }

            var vendorDetailsList = await _dbConnection.QueryAsync<ExistingVendorDetails>(
                "dbo.GetVendorDetails", 
                parameters, 
                commandType: CommandType.StoredProcedure
            );

            if (!vendorDetailsList.Any())
            {
                Log.Information("No data returned from stored procedure!");
            }

                return vendorDetailsList?.ToList() ?? new List<ExistingVendorDetails>();
        }
         public async Task<bool> ActiveAMCValidation(int AssetId, int? id = null)
        {   

              var query = "SELECT COUNT(1) FROM FixedAsset.AssetAmc   WHERE AssetId = @AssetId AND IsDeleted = 0 AND IsActive = 1 ";
                var parameters = new DynamicParameters(new { AssetId = AssetId });

             if (id is not null)
             {
                 query += " AND Id != @Id";
                 parameters.Add("Id", id);
             }
                var count = await _dbConnection.ExecuteScalarAsync<int>(query, parameters);
                return count > 0;
        }

    
    }
}