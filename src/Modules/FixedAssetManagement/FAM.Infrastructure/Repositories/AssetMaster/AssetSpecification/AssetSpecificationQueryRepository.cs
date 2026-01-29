
using System.Data;
using System.Text.Json;
using FAM.Application.AssetMaster.AssetSpecification.Queries.GetAssetSpecification;
using FAM.Application.AssetMaster.AssetSpecification.Queries.GetAssetSpecificationBasedMachineNo;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetSpecification;
using Dapper;

namespace FAM.Infrastructure.Repositories.AssetMaster.AssetSpecification
{
    public class AssetSpecificationQueryRepository : IAssetSpecificationQueryRepository    
    {
          private readonly IDbConnection _dbConnection;
        public AssetSpecificationQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }     
        //public async Task<(List<AssetSpecificationDTO>, int)> GetAllAssetSpecificationAsync(int PageNumber, int PageSize, string? SearchTerm)
                    
   public async Task<(List<AssetSpecificationJsonDto>, int)> GetAllAssetSpecificationAsync(int PageNumber, int PageSize, string? SearchTerm)
    {
        var parameters = new
        {
            PageNumber = (PageNumber - 1) * PageSize,
            PageSize,
            SearchTerm = string.IsNullOrEmpty(SearchTerm) ? null : $"%{SearchTerm}%"
        };

        var assetDictionary = new Dictionary<int, AssetSpecificationJsonDto>();
            var query = @"
            SELECT 
            s.AssetId, a.AssetCode, a.AssetName,
            s.SpecificationId, SM.SpecificationName , s.SpecificationValue ,SM.IsDefault
            FROM FixedAsset.AssetSpecifications S  
            INNER JOIN FixedAsset.AssetMaster A ON A.Id = S.AssetId   
            INNER JOIN FixedAsset.SpecificationMaster SM ON SM.Id = S.SpecificationId  
            WHERE A.IsDeleted = 0  
            and (@SearchTerm IS NULL OR a.AssetName LIKE @SearchTerm OR a.AssetCode LIKE @SearchTerm)
            ORDER BY s.AssetId
            OFFSET @PageNumber ROWS FETCH NEXT @PageSize ROWS ONLY;

            SELECT COUNT(*) 
            FROM FixedAsset.AssetSpecifications S  
            INNER JOIN FixedAsset.AssetMaster A ON A.Id = S.AssetId   
            INNER JOIN FixedAsset.SpecificationMaster SM ON SM.Id = S.SpecificationId  
            WHERE A.IsDeleted = 0  
            and (@SearchTerm IS NULL OR SM.SpecificationName LIKE @SearchTerm OR s.SpecificationValue  LIKE @SearchTerm  OR a.AssetName LIKE @SearchTerm OR a.AssetCode LIKE @SearchTerm);
            ";

        List<dynamic> assetResult;
        int totalRecords;

        using (var multi = await _dbConnection.QueryMultipleAsync(query, parameters))
        {
            // Read all results before processing
            assetResult = (await multi.ReadAsync()).ToList();
            totalRecords = await multi.ReadSingleAsync<int>();
        }

        // Process all data after reading from the GridReader
        foreach (var row in assetResult)
        {
            int assetId = row.AssetId;

            if (!assetDictionary.TryGetValue(assetId, out var assetEntry))
            {
                assetEntry = new AssetSpecificationJsonDto
                {
                    AssetId = assetId,
                    AssetCode = row.AssetCode,
                    AssetName = row.AssetName,
                    Specifications = new List<SpecificationDto>()
                };
                
                assetDictionary.Add(assetId, assetEntry);
            }

            // If the row contains specification data, add it to the specifications list
            if (row.SpecificationId != null)
            {
                assetEntry.Specifications.Add(new SpecificationDto
                {
                    SpecificationId = row.SpecificationId,
                    SpecificationName = row.SpecificationName,
                    SpecificationValue = row.SpecificationValue,
                    ISDefault = row.IsDefault
                });
            }
        }

        return (assetDictionary.Values.ToList(), totalRecords);
    }

        public async Task<(List<AssetSpecBasedOnMachineNoDto>, int)> GetAssetSpecBasedOnMachineNos(int PageNumber, int PageSize, string? SearchTerm)
        {
            var query = $$"""
                DECLARE @TotalCount INT;

                SELECT @TotalCount = COUNT(*)
                FROM FixedAsset.SpecificationMaster A
                INNER JOIN FixedAsset.AssetSpecifications B ON A.Id = B.SpecificationId
                LEFT JOIN FixedAsset.AssetPurchaseDetails C ON B.AssetId = C.AssetId
                {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (A.SpecificationName LIKE @Search OR CAST(B.AssetId AS VARCHAR) LIKE @Search)")}};
                
                SELECT 
                    B.AssetId,
                    A.SpecificationName,
                    B.SpecificationValue,
                    C.CapitalizationDate
                FROM FixedAsset.SpecificationMaster A
                INNER JOIN FixedAsset.AssetSpecifications B ON A.Id = B.SpecificationId
                LEFT JOIN FixedAsset.AssetPurchaseDetails C ON B.AssetId = C.AssetId
                {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (A.SpecificationName LIKE @Search OR CAST(B.AssetId AS VARCHAR) LIKE @Search)")}}
                ORDER BY B.AssetId DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;
            """;

            var parameters = new
            {
                Search = $"%{SearchTerm}%",
                Offset = (PageNumber - 1) * PageSize,
                PageSize
            };

            var result = await _dbConnection.QueryMultipleAsync(query, parameters);
            var list = (await result.ReadAsync<AssetSpecBasedOnMachineNoDto>()).ToList();
            int totalCount = await result.ReadFirstAsync<int>();

            return (list, totalCount);
        }

        public async Task<List<AssetSpecificationJsonDto>> GetByAssetSpecificationNameAsync(string searchPattern)
        {
            var parameters = new
            {
                SearchPattern = string.IsNullOrEmpty(searchPattern) ? null : $"%{searchPattern}%"
            };

            var assetDictionary = new Dictionary<int, AssetSpecificationJsonDto>();


            var query = @"
                SELECT 
                s.AssetId, a.AssetCode, a.AssetName,
                s.SpecificationId, SM.SpecificationName , s.SpecificationValue  ,SM.IsDefault
                FROM FixedAsset.AssetSpecifications S  
                INNER JOIN FixedAsset.AssetMaster A ON A.Id = S.AssetId   
                INNER JOIN FixedAsset.SpecificationMaster SM ON SM.Id = S.SpecificationId  
                WHERE A.IsDeleted = 0  
                and (@searchPattern IS NULL OR sM.SpecificationName LIKE @searchPattern OR s.SpecificationValue  LIKE @searchPattern OR a.AssetName LIKE @searchPattern OR a.AssetCode LIKE @searchPattern)
                ORDER BY s.AssetId ";

            List<dynamic> assetResult;
            using (var multi = await _dbConnection.QueryMultipleAsync(query, parameters))
            {
                assetResult = (await multi.ReadAsync()).ToList();
            }

            foreach (var row in assetResult)
            {
                int assetId = row.AssetId;

                if (!assetDictionary.TryGetValue(assetId, out var assetEntry))
                {
                    assetEntry = new AssetSpecificationJsonDto
                    {
                        AssetId = assetId,
                        AssetCode = row.AssetCode,
                        AssetName = row.AssetName,
                        Specifications = new List<SpecificationDto>()
                    };
                    assetDictionary.Add(assetId, assetEntry);
                }

                if (row.SpecificationId != null)
                {
                    assetEntry.Specifications.Add(new SpecificationDto
                    {
                        SpecificationId = row.SpecificationId,
                        SpecificationName = row.SpecificationName,
                        SpecificationValue = row.SpecificationValue,
                        ISDefault = row.IsDefault
                    });
                }
            }
            return assetDictionary.Values.ToList();
        }
        public async Task<AssetSpecificationJsonDto> GetByIdAsync(int assetId)
        {
            var parameters = new { AssetId = assetId };

            var query = @"
                SELECT 
                s.AssetId, a.AssetCode, a.AssetName,
                s.SpecificationId, SM.SpecificationName , s.SpecificationValue  ,SM.IsDefault
                FROM FixedAsset.AssetSpecifications S  
                INNER JOIN FixedAsset.AssetMaster A ON A.Id = S.AssetId   
                INNER JOIN FixedAsset.SpecificationMaster SM ON SM.Id = S.SpecificationId                    
                WHERE A.IsDeleted = 0  
                and  A.Id =@assetId 
                ORDER BY s.AssetId ";
            
            AssetSpecificationJsonDto? assetEntry = null;

            using (var multi = await _dbConnection.QueryMultipleAsync(query, parameters))
            {
                var assetResult = (await multi.ReadAsync()).ToList();

                if (assetResult.Count > 0)
                {
                    // Extracting the first row (assuming the same AssetId for all rows)
                    var firstRow = assetResult.First();

                    // Create the asset entry
                    assetEntry = new AssetSpecificationJsonDto
                    {
                        AssetId = firstRow.AssetId,
                        AssetCode = firstRow.AssetCode,
                        AssetName = firstRow.AssetName,
                        Specifications = new List<SpecificationDto>()
                    };

                    // Add specifications if available
                    foreach (var row in assetResult)
                    {
                        if (row.SpecificationId != null)
                        {
                            assetEntry.Specifications.Add(new SpecificationDto
                            {
                                SpecificationId = row.SpecificationId,
                                SpecificationName = row.SpecificationName,
                                SpecificationValue = row.SpecificationValue,
                                ISDefault = row.IsDefault
                            });
                        }
                    }
                }
            }
            return assetEntry ?? new AssetSpecificationJsonDto(); 
        }

        public async Task<bool> SoftDeleteValidation(int Id)
        {
            const string query = @"
                SELECT  AM.Id
                FROM FixedAsset.AssetMaster AM
                inner join  FixedAsset.AssetSpecifications AP on AP.AssetId = AM.Id
                WHERE AP.Id = @Id AND   AM.IsDeleted = 0;";        
            using var multi = await _dbConnection.QueryMultipleAsync(query, new { Id = Id });        
            var warrantyExists = await multi.ReadFirstOrDefaultAsync<int?>();          
            return warrantyExists.HasValue ;
        }

        
    }
}