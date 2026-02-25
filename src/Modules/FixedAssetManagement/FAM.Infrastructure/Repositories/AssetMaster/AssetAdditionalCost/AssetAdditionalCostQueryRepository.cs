using System.Data;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetAdditionalCost;
using FAM.Domain.Common;
using Dapper;

namespace FAM.Infrastructure.Repositories.AssetMaster.AssetAdditionalCost
{
    public class AssetAdditionalCostQueryRepository : IAssetAdditionalCostQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        public AssetAdditionalCostQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<(List<FAM.Domain.Entities.AssetPurchase.AssetAdditionalCost>, int)> GetAllAdditionalCostGroupAsync(int PageNumber, int PageSize, string? SearchTerm)
        {
                var query = $$"""
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*) 
                FROM FixedAsset.AssetAdditionalCost
                {{(string.IsNullOrEmpty(SearchTerm) ? "" : "WHERE AssetId LIKE @Search OR AssetSourceId LIKE @Search")}};
                
                SELECT 
                    Id, 
                    AssetId,
                    AssetSourceId,
                    Amount,
                    JournalNo,
                    CostType
                FROM FixedAsset.AssetAdditionalCost 
                {{(string.IsNullOrEmpty(SearchTerm) ? "" : "WHERE AssetId LIKE @Search OR AssetSourceId LIKE @Search")}}
                ORDER BY Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;
                """;

                var parameters = new
                {
                    Search = $"%{SearchTerm}%",
                    Offset = (PageNumber - 1) * PageSize,
                    PageSize
                };

                var assetadditionalcost = await _dbConnection.QueryMultipleAsync(query, parameters);
                var assetAdditionalCostslist = (await assetadditionalcost.ReadAsync<FAM.Domain.Entities.AssetPurchase.AssetAdditionalCost>()).ToList();
                int totalCount = (await assetadditionalcost.ReadFirstAsync<int>());
                
                return (assetAdditionalCostslist, totalCount);
        }

        public async Task<FAM.Domain.Entities.AssetPurchase.AssetAdditionalCost?> GetByIdAsync(int Id)
        {
             const string query = @"
                    SELECT * 
                    FROM FixedAsset.AssetAdditionalCost 
                    WHERE AssetId = @Id";

                    var assetAdditionalCost = await _dbConnection.QueryFirstOrDefaultAsync<FAM.Domain.Entities.AssetPurchase.AssetAdditionalCost>(query, new { Id });
                    return assetAdditionalCost;
        }

        public async Task<List<FAM.Domain.Entities.MiscMaster>> GetCostTypeAsync()
        {
            const string query = @"
            SELECT M.Id,MiscTypeId,Code,M.Description,SortOrder,  M.IsActive
            ,M.CreatedBy,M.CreatedDate,M.CreatedByName,M.CreatedIP,M.ModifiedBy,M.ModifiedDate,M.ModifiedByName,M.ModifiedIP
            FROM FixedAsset.MiscMaster M
            INNER JOIN FixedAsset.MiscTypeMaster T on T.ID=M.MiscTypeId
            WHERE (MiscTypeCode = @MiscTypeCode) 
            AND  M.IsDeleted=0 and M.IsActive=1
            ORDER BY M.ID DESC";    
            var parameters = new { MiscTypeCode = MiscEnumEntity.Asset_CostType.MiscCode };        
            var result = await _dbConnection.QueryAsync<FAM.Domain.Entities.MiscMaster>(query,parameters);
            return result.ToList();
        }
    }
}