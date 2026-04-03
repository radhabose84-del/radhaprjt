#nullable disable
using System.Data;
using InventoryManagement.Application.Common.Interfaces.IMiscMaster;
using Dapper;

namespace InventoryManagement.Infrastructure.Repositories.MiscMaster
{
    public class MiscMasterQueryRepository  : IMiscMasterQueryRepository
    {
    private readonly IDbConnection _dbConnection;

     public MiscMasterQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;

        }

        public async Task<(List<InventoryManagement.Domain.Entities.MiscMaster>,int)> GetAllMiscMasterAsync(int PageNumber, int PageSize, string SearchTerm)
        {
                var query = $$"""
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*) 
                FROM [Inventory].[MiscMaster] M
                WHERE M.IsDeleted = 0
                {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (M.Code LIKE @Search)")}}; 

                SELECT M.Id, M.MiscTypeId, M.Code, M.Description, M.SortOrder, M.IsActive, M.IsDeleted, 
                    M.CreatedBy, M.CreatedDate, M.CreatedByName, M.CreatedIP, M.ModifiedBy, M.ModifiedDate, 
                    M.ModifiedByName, M.ModifiedIP
                FROM Inventory.MiscMaster M
                WHERE M.IsDeleted = 0 
                {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (M.Code LIKE @Search)")}}
                ORDER BY M.Id DESC 
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
            
            // Read the data for MiscMaster and convert to list
            var miscMasterList = (await result.ReadAsync<InventoryManagement.Domain.Entities.MiscMaster>()).ToList();
            
            // Read the total count
            int totalCount = await result.ReadFirstAsync<int>();

            return (miscMasterList, totalCount);
                
            }

            
            public async Task<InventoryManagement.Domain.Entities.MiscMaster> GetByIdAsync(int id)
        {            
           const string query = @" SELECT Id,MiscTypeId,Code,Description,SortOrder,IsActive  FROM Inventory.MiscMaster          
             WHERE Id = @id AND IsDeleted = 0 ";                          
            return await _dbConnection.QueryFirstOrDefaultAsync<InventoryManagement.Domain.Entities.MiscMaster>(query, new { id });
        } 


        public async Task<List<InventoryManagement.Domain.Entities.MiscMaster>>  GetMiscMaster(string searchPattern,string miscTypeCode,string miscTypeDesc)
        {
            

            const string query = @"SELECT M.Id, M.Code, M.Description
                        FROM Inventory.MiscMaster AS M
                        INNER JOIN Inventory.MiscTypeMaster AS MT ON MT.Id = M.MiscTypeId
                        WHERE M.IsDeleted = 0
                        AND MT.IsDeleted = 0
                        AND M.IsActive  = 1
                        AND (@MiscTypeCode IS NULL OR MT.MiscTypeCode LIKE @MiscTypeCode)
                        AND (
                            @SearchPattern IS NULL
                            OR M.Code LIKE @SearchPattern
                            OR M.Description LIKE @SearchPattern
                            )    AND (@MiscTypeDesc IS NULL OR MT.Description LIKE @MiscTypeDesc);";
                
            
            var parameters = new
            {
                SearchPattern = string.IsNullOrWhiteSpace(searchPattern) ? null : $"%{searchPattern}%",
                MiscTypeCode  = string.IsNullOrWhiteSpace(miscTypeCode)  ? null : $"%{miscTypeCode}%",
                MiscTypeDesc  = string.IsNullOrWhiteSpace(miscTypeDesc)  ? null : $"%{miscTypeDesc}%"
             
              };

            var miscmaster = await _dbConnection.QueryAsync<InventoryManagement.Domain.Entities.MiscMaster>(query, parameters);
            return miscmaster.ToList();
        }

        public async Task<InventoryManagement.Domain.Entities.MiscMaster> GetByMiscMasterCodeAsync(string name, int? id = null)
        {
              var query = """
                 SELECT * FROM Inventory.MiscMaster
                 WHERE Code = @Name AND IsDeleted = 0 
                 """;

             var parameters = new DynamicParameters(new { Name = name });

             if (id is not null)
             {
                 query += " AND Id != @Id";
                 parameters.Add("Id", id);
             }

            return await _dbConnection.QueryFirstOrDefaultAsync<InventoryManagement.Domain.Entities.MiscMaster>(query, parameters);
        } 

               public async Task<int> GetMaxSortOrderAsync()
       {
           var query = "SELECT ISNULL(MAX(SortOrder), 0) FROM Inventory.MiscMaster WHERE IsDeleted = 0 ";
           return await _dbConnection.QueryFirstOrDefaultAsync<int>(query);
       }
        
        public async Task<bool> AlreadyExistsAsync(string code, int miscTypeId, int? id = null)
        {
            var query = @"SELECT COUNT(1) 
                        FROM Inventory.MiscMaster 
                        WHERE Code = @Code 
                            AND MiscTypeId = @MiscTypeId  
                            AND IsDeleted = 0 
                            ";

            var parameters = new DynamicParameters(new 
            { 
                Code = code, 
                MiscTypeId = miscTypeId 
            });

            if (id is not null)
            {
                query += " AND Id != @Id";
                parameters.Add("Id", id);
            }

            var count = await _dbConnection.ExecuteScalarAsync<int>(query, parameters);
            return count > 0;
        }      


           public async Task<bool> NotFoundAsync(int id)
        {
             var query = "SELECT COUNT(1) FROM Inventory.MiscMaster WHERE Id = @Id AND IsDeleted = 0";
             
                var count = await _dbConnection.ExecuteScalarAsync<int>(query, new { Id = id });
                return count > 0;
        } 

           public async Task<bool> FKColumnValidation(int MiscMasterId)
        {
            var query = "SELECT COUNT(1) FROM Inventory.MiscMaster WHERE Id = @Id AND IsDeleted = 0   ";
             
                var count = await _dbConnection.ExecuteScalarAsync<int>(query, new { Id = MiscMasterId });
                return count > 0;
        }
          public async Task<InventoryManagement.Domain.Entities.MiscMaster>  GetMiscMasterByName(string miscTypeCode,string miscTypeName)
        {
            

            const string query = @"SELECT M.Id,M.Code ,M.Description  FROM Inventory.MiscMaster AS M
                                INNER JOIN Inventory.MiscTypeMaster AS MT 
                                ON MT.Id = M.MiscTypeId
                                WHERE M.IsDeleted = 0 AND MT.IsDeleted = 0 AND M.IsActive = 1 AND MT.MiscTypeCode= @MiscTypeCode AND M.Code=@MiscTypeName  ";
                
            
            var parameters = new 
              { 
                  MiscTypeName = miscTypeName,
                  MiscTypeCode = miscTypeCode 
             
              };

            var miscmaster = await _dbConnection.QueryFirstOrDefaultAsync<InventoryManagement.Domain.Entities.MiscMaster>(query, parameters);
            return miscmaster;
        }      




        public async Task<bool> SoftDeleteValidation(int id)
        {
            const string query = @"
                SELECT CASE WHEN
                    EXISTS (SELECT 1 FROM [Inventory].[HSNMaster] WHERE TypeId = @id AND IsDeleted = 0)
                    OR EXISTS (SELECT 1 FROM [Inventory].[HSNMaster] WHERE GSTCategoryId = @id AND IsDeleted = 0)
                    OR EXISTS (SELECT 1 FROM [Inventory].[ItemMaster] WHERE ItemClassificationId = @id AND IsDeleted = 0)
                    OR EXISTS (SELECT 1 FROM [Inventory].[ItemMaster] WHERE XPlantMaterialStatusId = @id AND IsDeleted = 0)
                    OR EXISTS (SELECT 1 FROM [Inventory].[ItemMaster] WHERE IssueRuleId = @id AND IsDeleted = 0)
                    OR EXISTS (SELECT 1 FROM [Inventory].[UOM] WHERE UOMTypeId = @id AND IsDeleted = 0)
                    OR EXISTS (SELECT 1 FROM [Inventory].[PutAwayStrategy] WHERE StorageTypeId = @id AND IsDeleted = 0)
                    OR EXISTS (SELECT 1 FROM [Inventory].[PutAwayStrategy] WHERE PriorityId = @id AND IsDeleted = 0)
                    OR EXISTS (SELECT 1 FROM [Inventory].[ItemInventory] WHERE RequestTypeId = @id)
                    OR EXISTS (SELECT 1 FROM [Inventory].[ItemInventory] WHERE ValuationMethodId = @id)
                    OR EXISTS (SELECT 1 FROM [Inventory].[ItemInventory] WHERE DefaultMaterialRequestTypeId = @id)
                    OR EXISTS (SELECT 1 FROM [Inventory].[ItemManufacture] WHERE ManufacturingTypeId = @id)
                    OR EXISTS (SELECT 1 FROM [Inventory].[ItemPurchase] WHERE SourceOfItem = @id)
                    OR EXISTS (SELECT 1 FROM [Inventory].[ItemQuality] WHERE CertificateTypeId = @id)
                    OR EXISTS (SELECT 1 FROM [Inventory].[ItemSale] WHERE ValuationMethodId = @id)
                    OR EXISTS (SELECT 1 FROM [Inventory].[ItemVariantAttribute] WHERE VariantBasedOn = @id)
                    OR EXISTS (SELECT 1 FROM [Inventory].[ItemVariantAttribute] WHERE AttributeId = @id)
                THEN 1 ELSE 0 END;";

            var result = await _dbConnection.QueryFirstOrDefaultAsync<int>(query, new { id });
            return result == 1;
        }

        public async Task<bool> IsMiscMasterLinkedAsync(int id)
        {
            const string query = @"
                SELECT CASE WHEN
                    EXISTS (SELECT 1 FROM [Inventory].[HSNMaster] WHERE TypeId = @id AND IsDeleted = 0 AND IsActive = 1)
                    OR EXISTS (SELECT 1 FROM [Inventory].[HSNMaster] WHERE GSTCategoryId = @id AND IsDeleted = 0 AND IsActive = 1)
                    OR EXISTS (SELECT 1 FROM [Inventory].[ItemMaster] WHERE ItemClassificationId = @id AND IsDeleted = 0 AND IsActive = 1)
                    OR EXISTS (SELECT 1 FROM [Inventory].[ItemMaster] WHERE XPlantMaterialStatusId = @id AND IsDeleted = 0 AND IsActive = 1)
                    OR EXISTS (SELECT 1 FROM [Inventory].[ItemMaster] WHERE IssueRuleId = @id AND IsDeleted = 0 AND IsActive = 1)
                    OR EXISTS (SELECT 1 FROM [Inventory].[UOM] WHERE UOMTypeId = @id AND IsDeleted = 0 AND IsActive = 1)
                    OR EXISTS (SELECT 1 FROM [Inventory].[PutAwayStrategy] WHERE StorageTypeId = @id AND IsDeleted = 0 AND IsActive = 1)
                    OR EXISTS (SELECT 1 FROM [Inventory].[PutAwayStrategy] WHERE PriorityId = @id AND IsDeleted = 0 AND IsActive = 1)
                    OR EXISTS (SELECT 1 FROM [Inventory].[ItemInventory] WHERE RequestTypeId = @id)
                    OR EXISTS (SELECT 1 FROM [Inventory].[ItemInventory] WHERE ValuationMethodId = @id)
                    OR EXISTS (SELECT 1 FROM [Inventory].[ItemInventory] WHERE DefaultMaterialRequestTypeId = @id)
                    OR EXISTS (SELECT 1 FROM [Inventory].[ItemManufacture] WHERE ManufacturingTypeId = @id)
                    OR EXISTS (SELECT 1 FROM [Inventory].[ItemPurchase] WHERE SourceOfItem = @id)
                    OR EXISTS (SELECT 1 FROM [Inventory].[ItemQuality] WHERE CertificateTypeId = @id)
                    OR EXISTS (SELECT 1 FROM [Inventory].[ItemSale] WHERE ValuationMethodId = @id)
                    OR EXISTS (SELECT 1 FROM [Inventory].[ItemVariantAttribute] WHERE VariantBasedOn = @id)
                    OR EXISTS (SELECT 1 FROM [Inventory].[ItemVariantAttribute] WHERE AttributeId = @id)
                THEN 1 ELSE 0 END;";

            var result = await _dbConnection.QueryFirstOrDefaultAsync<int>(query, new { id });
            return result == 1;
        }

    }
}
