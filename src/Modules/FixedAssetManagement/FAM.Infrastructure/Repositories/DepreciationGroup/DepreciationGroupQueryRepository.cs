using System.Data;
using FAM.Application.Common.Interfaces.IDepreciationGroup;
using FAM.Application.DepreciationGroup.Queries.GetDepreciationGroup;
using FAM.Domain.Common;
using Dapper;

namespace FAM.Infrastructure.Repositories.DepreciationGroup
{
    public class DepreciationGroupQueryRepository : IDepreciationGroupQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        public DepreciationGroupQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }     
        public async Task<(List<DepreciationGroupDTO>, int)> GetAllDepreciationGroupAsync(int PageNumber, int PageSize, string? SearchTerm)
        {
             var query = $$"""
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*) 
                FROM FixedAsset.DepreciationGroups 
                WHERE IsDeleted = 0
                {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (Code LIKE @Search OR DepreciationGroupName LIKE @Search)")}};

                SELECT DG.Id,DG.Code,DG.BookType,DG.DepreciationGroupName,DG.AssetGroupId,DG.UsefulLife,DG.DepreciationMethod,DG.ResidualValue,DG.SortOrder,DG.IsActive
                ,DG.CreatedBy,DG.CreatedDate as CreatedAt,DG.CreatedByName,DG.CreatedIP,DG.ModifiedBy,DG.ModifiedDate,DG.ModifiedByName,DG.ModifiedIP
                ,MM.description BookTypeDesc,M.description DepreciationMethodDesc,AG.GroupName AssetGroupName
                FROM FixedAsset.DepreciationGroups DG
                INNER JOIN FixedAsset.MiscMaster MM on MM.Id =DG.BookType
                INNER JOIN FixedAsset.MiscMaster M on M.Id =DG.DepreciationMethod
                INNER JOIN FixedAsset.AssetGroup AG on AG.Id=DG.AssetGroupId
                WHERE DG.IsDeleted = 0
                {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (DG.Code LIKE @Search OR DG.DepreciationGroupName LIKE @Search )")}}
                ORDER BY DG.Id desc
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
                SELECT @TotalCount AS TotalCount;
                """;
            var parameters = new
                       {
                           Search = $"%{SearchTerm}%",
                           Offset = (PageNumber - 1) * PageSize,
                           PageSize
                       };

            var depreciationGroups = await _dbConnection.QueryMultipleAsync(query, parameters);
            var depreciationGroupList = (await depreciationGroups.ReadAsync<DepreciationGroupDTO>()).ToList();
            int totalCount = (await depreciationGroups.ReadFirstAsync<int>());             
            return (depreciationGroupList, totalCount);             
        }

        public async Task<List<DepreciationGroupDTO>> GetByDepreciationNameAsync(string searchPattern)
        {
            const string query = @"
            SELECT DG.Id,DG.Code,DG.BookType,DG.DepreciationGroupName,DG.AssetGroupId,DG.UsefulLife,DG.DepreciationMethod,DG.ResidualValue,DG.SortOrder,DG.IsActive
            ,DG.CreatedBy,DG.CreatedDate,DG.CreatedByName,DG.CreatedIP,DG.ModifiedBy,DG.ModifiedDate,DG.ModifiedByName,DG.ModifiedIP
            ,MM.description BookTypeDesc,M.description DepreciationMethodDesc,AG.GroupName AssetGroupName
            FROM FixedAsset.DepreciationGroups DG
            INNER JOIN FixedAsset.MiscMaster MM on MM.Id =DG.BookType
            INNER JOIN FixedAsset.MiscMaster M on M.Id =DG.DepreciationMethod    
            INNER JOIN FixedAsset.AssetGroup AG on AG.Id=DG.AssetGroupId        
            WHERE (DG.DepreciationGroupName LIKE @SearchPattern OR DG.Code LIKE @SearchPattern) 
            AND  DG.IsDeleted=0 and DG.IsActive=1
            ORDER BY ID DESC";            
            var result = await _dbConnection.QueryAsync<DepreciationGroupDTO>(query, new { SearchPattern = $"%{searchPattern}%" });
            return result.ToList();
        }

        public async Task<DepreciationGroupDTO> GetByIdAsync(int depGroupId)
        {
            const string query = @"
            SELECT DG.Id,DG.Code,DG.BookType,DG.DepreciationGroupName,DG.AssetGroupId,DG.UsefulLife,DG.DepreciationMethod,DG.ResidualValue,DG.SortOrder,DG.IsActive
            ,DG.CreatedBy,DG.CreatedDate,DG.CreatedByName,DG.CreatedIP,DG.ModifiedBy,DG.ModifiedDate,DG.ModifiedByName,DG.ModifiedIP
            ,MM.description BookTypeDesc,M.description DepreciationMethodDesc,AG.GroupName AssetGroupName
            FROM FixedAsset.DepreciationGroups DG
            INNER JOIN FixedAsset.MiscMaster MM on MM.Id =DG.BookType
            INNER JOIN FixedAsset.MiscMaster M on M.Id =DG.DepreciationMethod 
            INNER JOIN FixedAsset.AssetGroup AG on AG.Id=DG.AssetGroupId
            WHERE DG.Id = @depGroupId AND DG.IsDeleted=0";
            var depreciationGroups = await _dbConnection.QueryFirstOrDefaultAsync<DepreciationGroupDTO>(query, new { depGroupId });           
            if (depreciationGroups is null)
            {
                throw new KeyNotFoundException($"DepreciationGroup with ID {depGroupId} not found.");
            }
            return depreciationGroups;
        }

        public async Task<List<FAM.Domain.Entities.MiscMaster>> GetDepreciationMethodAsync()
        {
            const string query = @"
            SELECT M.Id,MiscTypeId,Code,M.Description,SortOrder,  M.IsActive
            ,M.CreatedBy,M.CreatedDate,M.CreatedByName,M.CreatedIP,M.ModifiedBy,M.ModifiedDate,M.ModifiedByName,M.ModifiedIP
            FROM FixedAsset.MiscMaster M
            INNER JOIN FixedAsset.MiscTypeMaster T on T.ID=M.MiscTypeId
            WHERE (MiscTypeCode = @MiscTypeCode) 
            AND  M.IsDeleted=0 and M.IsActive=1
            ORDER BY M.ID DESC";          
            var parameters = new { MiscTypeCode = MiscEnumEntity.Depreciation_DepMethod.MiscCode };     
            var result = await _dbConnection.QueryAsync<FAM.Domain.Entities.MiscMaster>(query,parameters);
            return result.ToList();
        }
        
        public async Task<List<FAM.Domain.Entities.MiscMaster>> GetBookTypeAsync()
        {
            const string query = @"
            SELECT M.Id,MiscTypeId,Code,M.Description,SortOrder,  M.IsActive
            ,M.CreatedBy,M.CreatedDate,M.CreatedByName,M.CreatedIP,M.ModifiedBy,M.ModifiedDate,M.ModifiedByName,M.ModifiedIP
            FROM FixedAsset.MiscMaster M
            INNER JOIN FixedAsset.MiscTypeMaster T on T.ID=M.MiscTypeId
            WHERE (MiscTypeCode = @MiscTypeCode) 
            AND  M.IsDeleted=0 and M.IsActive=1
            ORDER BY M.ID DESC";    
            var parameters = new { MiscTypeCode = MiscEnumEntity.Depreciation_BookType.MiscCode };        
            var result = await _dbConnection.QueryAsync<FAM.Domain.Entities.MiscMaster>(query,parameters);
            return result.ToList();
        }
        
        public async Task<bool> SoftDeleteValidation(int Id)
        {
            const string query = @"
                SELECT 1 
                FROM FixedAsset.AssetMaster AM
                inner join  FixedAsset.DepreciationGroups DG on DG.AssetGroupId = AM.AssetGroupId
                WHERE DG.Id = @Id AND   AM.IsDeleted = 0;";        
            using var multi = await _dbConnection.QueryMultipleAsync(query, new { Id = Id });        
            var DepreciationExists = await multi.ReadFirstOrDefaultAsync<int?>();          
            return DepreciationExists.HasValue ;
        }
    }
}