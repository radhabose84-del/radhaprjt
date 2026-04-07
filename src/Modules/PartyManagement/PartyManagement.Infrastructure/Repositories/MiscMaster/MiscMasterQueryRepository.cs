#nullable disable
using System.Data;
using PartyManagement.Application.Common.Interfaces.IMiscMaster;
using Dapper;

namespace PartyManagement.Infrastructure.Repositories.MiscMaster
{
    public class MiscMasterQueryRepository : IMiscMasterQueryRepository
    {
        private readonly IDbConnection _dbConnection;

        public MiscMasterQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;

        }
        public async Task<(List<PartyManagement.Domain.Entities.MiscMaster>,int)> GetAllMiscMasterAsync(int PageNumber, int PageSize, string SearchTerm)
        {
                var query = $$"""
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*) 
                FROM [Party].[MiscMaster] M
                WHERE M.IsDeleted = 0
                {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (M.Code LIKE @Search)")}}; 

                SELECT M.Id, M.MiscTypeId, M.Code, M.Description, M.SortOrder, M.IsActive, M.IsDeleted, 
                    M.CreatedBy, M.CreatedDate, M.CreatedByName, M.CreatedIP, M.ModifiedBy, M.ModifiedDate, 
                    M.ModifiedByName, M.ModifiedIP
                FROM Party.MiscMaster M
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
            var miscMasterList = (await result.ReadAsync<PartyManagement.Domain.Entities.MiscMaster>()).ToList();
            
            // Read the total count
            int totalCount = await result.ReadFirstAsync<int>();

            return (miscMasterList, totalCount);
                
            }

            
            public async Task<PartyManagement.Domain.Entities.MiscMaster> GetByIdAsync(int id)
        {            
           const string query = @" SELECT Id,MiscTypeId,Code,Description,SortOrder,IsActive  FROM Party.MiscMaster          
             WHERE Id = @id AND IsDeleted = 0 ";                          
            return await _dbConnection.QueryFirstOrDefaultAsync<PartyManagement.Domain.Entities.MiscMaster>(query, new { id });
        } 


        public async Task<List<PartyManagement.Domain.Entities.MiscMaster>>  GetMiscMaster(string searchPattern,string miscTypeCode)
        {
            

            const string query = @"SELECT M.Id,M.Code ,M.Description  FROM Party.MiscMaster M
            INNER JOIN [Party].[MiscTypeMaster] MT ON MT.Id = M.MiscTypeId
                WHERE M.IsDeleted = 0 AND MT.IsDeleted = 0 AND M.IsActive = 1  AND MT.MiscTypeCode= @MiscTypeCode AND M.Code LIKE @SearchPattern  ";
                
            
            var parameters = new 
              { 
                  SearchPattern = $"%{searchPattern ?? string.Empty}%",
                  MiscTypeCode = miscTypeCode 
             
              };

            var miscmaster = await _dbConnection.QueryAsync<PartyManagement.Domain.Entities.MiscMaster>(query, parameters);
            return miscmaster.ToList();
        }

        public async Task<PartyManagement.Domain.Entities.MiscMaster> GetByMiscMasterCodeAsync(string name, int? id = null)
        {
              var query = """
                 SELECT * FROM Party.MiscMaster
                 WHERE Code = @Name AND IsDeleted = 0 
                 """;

             var parameters = new DynamicParameters(new { Name = name });

             if (id is not null)
             {
                 query += " AND Id != @Id";
                 parameters.Add("Id", id);
             }

            return await _dbConnection.QueryFirstOrDefaultAsync<PartyManagement.Domain.Entities.MiscMaster>(query, parameters);
        } 

               public async Task<int> GetMaxSortOrderAsync()
       {
           var query = "SELECT ISNULL(MAX(SortOrder), 0) FROM Party.MiscMaster WHERE IsDeleted = 0 ";
           return await _dbConnection.QueryFirstOrDefaultAsync<int>(query);
       }
        
        public async Task<bool> AlreadyExistsAsync(string code, int miscTypeId, int? id = null)
        {
            var query = @"SELECT COUNT(1) 
                        FROM Party.MiscMaster 
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
             var query = "SELECT COUNT(1) FROM Party.MiscMaster WHERE Id = @Id AND IsDeleted = 0";
             
                var count = await _dbConnection.ExecuteScalarAsync<int>(query, new { Id = id });
                return count > 0;
        } 

           public async Task<bool> FKColumnValidation(int MiscMasterId)
        {
            var query = "SELECT COUNT(1) FROM Party.MiscMaster WHERE Id = @Id AND IsDeleted = 0   ";
             
                var count = await _dbConnection.ExecuteScalarAsync<int>(query, new { Id = MiscMasterId });
                return count > 0;
        }
          public async Task<PartyManagement.Domain.Entities.MiscMaster>  GetMiscMasterByName(string miscTypeCode,string miscTypeName)
        {


            const string query = @"SELECT M.Id,M.Code ,M.Description  FROM Party.MiscMaster AS M
                                INNER JOIN Party.MiscTypeMaster AS MT
                                ON MT.Id = M.MiscTypeId
                                WHERE M.IsDeleted = 0 AND MT.IsDeleted = 0 AND M.IsActive = 1 AND MT.MiscTypeCode= @MiscTypeCode AND M.Code=@MiscTypeName  ";


            var parameters = new
              {
                  MiscTypeName = miscTypeName,
                  MiscTypeCode = miscTypeCode

              };

            var miscmaster = await _dbConnection.QueryFirstOrDefaultAsync<PartyManagement.Domain.Entities.MiscMaster>(query, parameters);
            return miscmaster;
        }

        public async Task<bool> SoftDeleteValidationAsync(int id)
        {
            const string query = @"
                SELECT CASE WHEN
                    EXISTS (SELECT 1 FROM Party.PartyMaster WHERE PartyZoneId = @id AND IsDeleted = 0)
                    OR EXISTS (SELECT 1 FROM Party.PartyMaster WHERE RegistrationTypeId = @id AND IsDeleted = 0)
                    OR EXISTS (SELECT 1 FROM Party.PartyMaster WHERE MSMETypeId = @id AND IsDeleted = 0)
                    OR EXISTS (SELECT 1 FROM Party.PartyMaster WHERE PayementModeId = @id AND IsDeleted = 0)
                    OR EXISTS (SELECT 1 FROM Party.PartyMaster WHERE DueDateTypeId = @id AND IsDeleted = 0)
                    OR EXISTS (SELECT 1 FROM Party.PartyMaster WHERE CustomerTypeId = @id AND IsDeleted = 0)
                    OR EXISTS (SELECT 1 FROM Party.PartyMaster WHERE StatusId = @id AND IsDeleted = 0)
                    OR EXISTS (SELECT 1 FROM Party.BankAccount WHERE AccountTypeId = @id AND IsDeleted = 0)
                    OR EXISTS (SELECT 1 FROM Party.BankAccount WHERE BranchId = @id AND IsDeleted = 0)
                    OR EXISTS (SELECT 1 FROM Party.PartyGroup WHERE GroupTypeId = @id AND IsDeleted = 0)
                    OR EXISTS (SELECT 1 FROM Party.PartyGroup WHERE GlCategoryId = @id AND IsDeleted = 0)
                    OR EXISTS (SELECT 1 FROM Party.PartyContact WHERE GenderId = @id)
                    OR EXISTS (SELECT 1 FROM Party.PartyContact WHERE PreferredChannelId = @id)
                    OR EXISTS (SELECT 1 FROM Party.PartyContact WHERE ContactTypeId = @id)
                    OR EXISTS (SELECT 1 FROM Party.PartyType WHERE PartyTypeId = @id)
                    OR EXISTS (SELECT 1 FROM Party.PartyDocument WHERE DocumentId = @id)
                    OR EXISTS (SELECT 1 FROM Party.PartyBank WHERE AccountTypeId = @id)
                    OR EXISTS (SELECT 1 FROM Party.SalesType WHERE ShippingConditionId = @id)
                    OR EXISTS (SELECT 1 FROM Party.SalesType WHERE AccountAssignmentId = @id)
                    OR EXISTS (SELECT 1 FROM Party.TransportDetail WHERE TransportModeId = @id)
                    OR EXISTS (SELECT 1 FROM Party.TransportDetail WHERE VehicleTypeId = @id)
                    OR EXISTS (SELECT 1 FROM Party.TransportDetail WHERE DefaultFreightTypeId = @id)
                    OR EXISTS (SELECT 1 FROM Party.AgentConfig WHERE SettlementCycleId = @id)
                THEN 1 ELSE 0 END;";

            return await _dbConnection.ExecuteScalarAsync<bool>(query, new { id });
        }

        public async Task<bool> IsMiscMasterLinkedAsync(int id)
        {
            const string query = @"
                SELECT CASE WHEN
                    EXISTS (SELECT 1 FROM Party.PartyMaster WHERE PartyZoneId = @id AND IsDeleted = 0 AND IsActive = 1)
                    OR EXISTS (SELECT 1 FROM Party.PartyMaster WHERE RegistrationTypeId = @id AND IsDeleted = 0 AND IsActive = 1)
                    OR EXISTS (SELECT 1 FROM Party.PartyMaster WHERE MSMETypeId = @id AND IsDeleted = 0 AND IsActive = 1)
                    OR EXISTS (SELECT 1 FROM Party.PartyMaster WHERE PayementModeId = @id AND IsDeleted = 0 AND IsActive = 1)
                    OR EXISTS (SELECT 1 FROM Party.PartyMaster WHERE DueDateTypeId = @id AND IsDeleted = 0 AND IsActive = 1)
                    OR EXISTS (SELECT 1 FROM Party.PartyMaster WHERE CustomerTypeId = @id AND IsDeleted = 0 AND IsActive = 1)
                    OR EXISTS (SELECT 1 FROM Party.PartyMaster WHERE StatusId = @id AND IsDeleted = 0 AND IsActive = 1)
                    OR EXISTS (SELECT 1 FROM Party.BankAccount WHERE AccountTypeId = @id AND IsDeleted = 0 AND IsActive = 1)
                    OR EXISTS (SELECT 1 FROM Party.BankAccount WHERE BranchId = @id AND IsDeleted = 0 AND IsActive = 1)
                    OR EXISTS (SELECT 1 FROM Party.PartyGroup WHERE GroupTypeId = @id AND IsDeleted = 0 AND IsActive = 1)
                    OR EXISTS (SELECT 1 FROM Party.PartyGroup WHERE GlCategoryId = @id AND IsDeleted = 0 AND IsActive = 1)
                    OR EXISTS (SELECT 1 FROM Party.PartyContact WHERE GenderId = @id)
                    OR EXISTS (SELECT 1 FROM Party.PartyContact WHERE PreferredChannelId = @id)
                    OR EXISTS (SELECT 1 FROM Party.PartyContact WHERE ContactTypeId = @id)
                    OR EXISTS (SELECT 1 FROM Party.PartyType WHERE PartyTypeId = @id)
                    OR EXISTS (SELECT 1 FROM Party.PartyDocument WHERE DocumentId = @id)
                    OR EXISTS (SELECT 1 FROM Party.PartyBank WHERE AccountTypeId = @id)
                    OR EXISTS (SELECT 1 FROM Party.SalesType WHERE ShippingConditionId = @id)
                    OR EXISTS (SELECT 1 FROM Party.SalesType WHERE AccountAssignmentId = @id)
                    OR EXISTS (SELECT 1 FROM Party.TransportDetail WHERE TransportModeId = @id)
                    OR EXISTS (SELECT 1 FROM Party.TransportDetail WHERE VehicleTypeId = @id)
                    OR EXISTS (SELECT 1 FROM Party.TransportDetail WHERE DefaultFreightTypeId = @id)
                    OR EXISTS (SELECT 1 FROM Party.AgentConfig WHERE SettlementCycleId = @id)
                THEN 1 ELSE 0 END;";

            return await _dbConnection.ExecuteScalarAsync<bool>(query, new { id });
        }

    }
}