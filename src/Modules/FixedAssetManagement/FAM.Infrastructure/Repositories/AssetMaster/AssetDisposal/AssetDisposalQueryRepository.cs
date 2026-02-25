using System.Data;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetDisposal;
using FAM.Domain.Common;
using Dapper;

namespace FAM.Infrastructure.Repositories.AssetMaster.AssetDisposal
{
    public class AssetDisposalQueryRepository : IAssetDisposalQueryRepository
    {
        private readonly IDbConnection _dbConnection; 
        public AssetDisposalQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<(List<FAM.Domain.Entities.AssetMaster.AssetDisposal>, int)> GetAllAssetDisposalAsync(int PageNumber, int PageSize, string? SearchTerm)
        {
            var query = $$"""
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*) 
                FROM FixedAsset.AssetDisposal
                {{(string.IsNullOrEmpty(SearchTerm) ? "" : "WHERE AssetId LIKE @Search OR AssetPurchaseId LIKE @Search")}};
                
                SELECT 
                    Id, 
                    AssetId,
                    AssetPurchaseId,
                    DisposalDate,
                    DisposalType,
                    DisposalReason,
                    DisposalAmount
                FROM FixedAsset.AssetDisposal 
                {{(string.IsNullOrEmpty(SearchTerm) ? "" : "WHERE AssetId LIKE @Search OR AssetPurchaseId LIKE @Search")}}
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

                var assetdisposal = await _dbConnection.QueryMultipleAsync(query, parameters);
                var assetDisposalslist = (await assetdisposal.ReadAsync<FAM.Domain.Entities.AssetMaster.AssetDisposal>()).ToList();
                int totalCount = (await assetdisposal.ReadFirstAsync<int>());
                
                return (assetDisposalslist, totalCount);
        }

        public async Task<FAM.Domain.Entities.AssetMaster.AssetDisposal?> GetByIdAsync(int Id)
        {
            const string query = @"
                    SELECT * 
                    FROM FixedAsset.AssetDisposal 
                    WHERE AssetId = @Id";

                    var assetdisposal = await _dbConnection.QueryFirstOrDefaultAsync<FAM.Domain.Entities.AssetMaster.AssetDisposal>(query, new { Id });
                    return assetdisposal;
        }

        public async Task<List<FAM.Domain.Entities.MiscMaster>> GetDisposalType()
        {
             const string query = @"
            SELECT M.Id,MiscTypeId,Code,M.Description,SortOrder,  M.IsActive
            ,M.CreatedBy,M.CreatedDate,M.CreatedByName,M.CreatedIP,M.ModifiedBy,M.ModifiedDate,M.ModifiedByName,M.ModifiedIP
            FROM FixedAsset.MiscMaster M
            INNER JOIN FixedAsset.MiscTypeMaster T on T.ID=M.MiscTypeId
            WHERE (MiscTypeCode = @MiscTypeCode) 
            AND  M.IsDeleted=0 and M.IsActive=1
            ORDER BY M.ID DESC";    
            var parameters = new { MiscTypeCode = MiscEnumEntity.Asset_DisposeType.MiscCode};        
            var result = await _dbConnection.QueryAsync<FAM.Domain.Entities.MiscMaster>(query,parameters);
            return result.ToList();
        }
    }
}