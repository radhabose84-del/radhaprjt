using System.Data;
using FAM.Application.AssetMaster.AssetWarranty.Queries.GetAssetWarranty;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetWarranty;
using FAM.Domain.Common;
using Dapper;

namespace FAM.Infrastructure.Repositories.AssetMaster.AssetWarranty
{
    public class AssetWarrantyQueryRepository : IAssetWarrantyQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        public AssetWarrantyQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<(List<AssetWarrantyDTO>, int)> GetAllAssetWarrantyAsync(int PageNumber, int PageSize, string? SearchTerm)
        {
            var query = $$"""
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*) 
                FROM FixedAsset.AssetWarranty A              
                WHERE A.IsDeleted = 0
                {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (AS.AssetCode LIKE @Search OR AssetName LIKE @Search  OR M.Description LIKE @Search OR A.Description LIKE @Search )")}};

                SELECT A.Id,A.AssetId,A.StartDate,A.EndDate,A.Period,A.WarrantyType,A.Description,A.ContactPerson,A.MobileNumber,A.Email,A.ServiceCountryId,A.ServiceStateId,A.ServiceCityId,
                A.ServiceAddressLine1,A.ServiceAddressLine2,A.ServicePinCode,A.ServiceContactPerson,A.ServiceMobileNumber,A.ServiceEmail,A.ServiceClaimProcessDescription,A.ServiceLastClaimDate,A.ServiceClaimStatus
                ,A.IsActive,A.CreatedBy,A.CreatedDate,A.CreatedByName,A.CreatedIP,A.ModifiedBy,A.ModifiedDate,A.ModifiedByName,A.ModifiedIP
                ,AM.AssetCode,AM.AssetName ,M.Description WarrantyTypeDesc,M1.Description WarrantyClaimStatus, '' CountryName,'' StateName,'' CityName
                FROM FixedAsset.AssetWarranty A
                INNER JOIN FixedAsset.AssetMaster AM on AM.Id=A.AssetId             
                LEFT JOIN FixedAsset.MiscMaster M on M.Id =A.WarrantyType
                LEFT JOIN FixedAsset.MiscMaster M1 on M1.Id =A.ServiceClaimStatus 
                WHERE A.IsDeleted = 0
                {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (AssetCode LIKE @Search OR AssetName LIKE @Search  OR M.Description LIKE @Search OR A.Description LIKE @Search )")}}
                ORDER BY A.Id desc
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
                SELECT @TotalCount AS TotalCount;
                """;
            var parameters = new
                       {
                           Search = $"%{SearchTerm}%",
                           Offset = (PageNumber - 1) * PageSize,
                           PageSize
                       };

            var assetWarranties= await _dbConnection.QueryMultipleAsync(query, parameters);
            var assetWarrantiesList = (await assetWarranties.ReadAsync<AssetWarrantyDTO>()).ToList();
            int totalCount = (await assetWarranties.ReadFirstAsync<int>());
            return (assetWarrantiesList, totalCount);
        }

        public async Task<List<AssetWarrantyDTO>> GetByAssetWarrantyNameAsync(string searchPattern)
        {
            const string query = @"
            SELECT A.Id,A.AssetId,A.StartDate,A.EndDate,A.Period,A.WarrantyType,A.Description,A.ContactPerson,A.MobileNumber,A.Email,A.ServiceCountryId,A.ServiceStateId,A.ServiceCityId,
            A.ServiceAddressLine1,A.ServiceAddressLine2,A.ServicePinCode,A.ServiceContactPerson,A.ServiceMobileNumber,A.ServiceEmail,A.ServiceClaimProcessDescription,A.ServiceLastClaimDate,A.ServiceClaimStatus
            ,A.IsActive,A.CreatedBy,A.CreatedDate,A.CreatedByName,A.CreatedIP,A.ModifiedBy,A.ModifiedDate,A.ModifiedByName,A.ModifiedIP
            ,AM.AssetCode,AM.AssetName ,M.Description WarrantyTypeDesc,M1.Description WarrantyClaimStatus
            FROM FixedAsset.AssetWarranty A
            INNER JOIN FixedAsset.AssetMaster AM on AM.Id=A.AssetId             
            LEFT JOIN FixedAsset.MiscMaster M on M.Id =A.WarrantyType
            LEFT JOIN FixedAsset.MiscMaster M1 on M1.Id =A.ServiceClaimStatus         
            WHERE  (AssetCode LIKE @searchPattern OR AssetName LIKE @searchPattern  OR A.Description LIKE @searchPattern OR M.Description LIKE @searchPattern )
            AND  A.IsDeleted=0 and A.IsActive=1
            ORDER BY A.ID DESC";            
            var result = await _dbConnection.QueryAsync<AssetWarrantyDTO>(query, new { SearchPattern = $"%{searchPattern}%" });
            return result.ToList();
        }

        public async Task<AssetWarrantyDTO> GetByIdAsync(int assetId)
        {
            const string query = @"
            SELECT A.Id,A.AssetId,A.StartDate,A.EndDate,A.Period,A.WarrantyType,A.Description,A.ContactPerson,A.MobileNumber,A.Email,A.ServiceCountryId,A.ServiceStateId,A.ServiceCityId,
            A.ServiceAddressLine1,A.ServiceAddressLine2,A.ServicePinCode,A.ServiceContactPerson,A.ServiceMobileNumber,A.ServiceEmail,A.ServiceClaimProcessDescription,A.ServiceLastClaimDate,A.ServiceClaimStatus
            ,A.IsActive,A.CreatedBy,A.CreatedDate,A.CreatedByName,A.CreatedIP,A.ModifiedBy,A.ModifiedDate,A.ModifiedByName,A.ModifiedIP
            ,AM.AssetCode,AM.AssetName ,M.Description WarrantyTypeDesc,M1.Description WarrantyClaimStatus
            FROM FixedAsset.AssetWarranty A
            INNER JOIN FixedAsset.AssetMaster AM on AM.Id=A.AssetId             
            LEFT JOIN FixedAsset.MiscMaster M on M.Id =A.WarrantyType
            LEFT JOIN FixedAsset.MiscMaster M1 on M1.Id =A.ServiceClaimStatus           
            WHERE A.Id = @assetId AND A.IsDeleted=0";
            var assetWarranties = await _dbConnection.QueryFirstOrDefaultAsync<AssetWarrantyDTO>(query, new { assetId });
            if (assetWarranties is null)
            {
                throw new KeyNotFoundException($"AssetWarranty with ID {assetId} not found.");
            }
            return assetWarranties;
        }
        public async Task<List<FAM.Domain.Entities.MiscMaster>> GetWarrantyTypeAsync()
        {
            const string query = @"
            SELECT M.Id,MiscTypeId,Code,M.Description,SortOrder,  M.IsActive
            ,M.CreatedBy,M.CreatedDate,M.CreatedByName,M.CreatedIP,M.ModifiedBy,M.ModifiedDate,M.ModifiedByName,M.ModifiedIP
            FROM FixedAsset.MiscMaster M
            INNER JOIN FixedAsset.MiscTypeMaster T on T.ID=M.MiscTypeId
            WHERE (MiscTypeCode = @MiscTypeCode) 
            AND  M.IsDeleted=0 and M.IsActive=1
            ORDER BY M.ID DESC";          
            var parameters = new { MiscTypeCode = MiscEnumEntity.Asset_WarrantyType.MiscCode };     
            var result = await _dbConnection.QueryAsync<FAM.Domain.Entities.MiscMaster>(query,parameters);
            return result.ToList();
        }
        public async Task<List<FAM.Domain.Entities.MiscMaster>> GetWarrantyClaimStatusAsync()
        {
            const string query = @"
            SELECT M.Id,MiscTypeId,Code,M.Description,SortOrder,  M.IsActive
            ,M.CreatedBy,M.CreatedDate,M.CreatedByName,M.CreatedIP,M.ModifiedBy,M.ModifiedDate,M.ModifiedByName,M.ModifiedIP
            FROM FixedAsset.MiscMaster M
            INNER JOIN FixedAsset.MiscTypeMaster T on T.ID=M.MiscTypeId
            WHERE (MiscTypeCode = @MiscTypeCode) 
            AND  M.IsDeleted=0 and M.IsActive=1
            ORDER BY M.ID DESC";          
            var parameters = new { MiscTypeCode = MiscEnumEntity.Asset_WarrantyClaimStatus.MiscCode };     
            var result = await _dbConnection.QueryAsync<FAM.Domain.Entities.MiscMaster>(query,parameters);
            return result.ToList();
        }

        public async Task<bool> SoftDeleteValidation(int Id)
        {
            const string query = @"
                SELECT 1
                FROM FixedAsset.AssetMaster AM
                INNER JOIN FixedAsset.AssetWarranty AW ON AW.AssetId = AM.Id
                WHERE AW.Id = @Id AND AM.IsDeleted = 0;";        
            using var multi = await _dbConnection.QueryMultipleAsync(query, new { Id = Id });        
            var warrantyExists = await multi.ReadFirstOrDefaultAsync<int?>();          
            return warrantyExists.HasValue ;
        }
        public async Task<string> GetBaseDirectoryAsync()
        {
            const string query = @"
            SELECT M.Description            
            FROM FixedAsset.MiscMaster M
            INNER JOIN FixedAsset.MiscTypeMaster T on T.ID=M.MiscTypeId
            WHERE (MiscTypeCode = @MiscTypeCode) 
            AND  M.IsDeleted=0 and M.IsActive=1
            ORDER BY M.ID DESC";    
            var parameters = new { MiscTypeCode = MiscEnumEntity.AssetWarrantyImage.MiscCode };        
            var result = await _dbConnection.QueryAsync<string>(query,parameters);
            return result.FirstOrDefault() ?? string.Empty;
        }
    }
}