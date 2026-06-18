using System.Data;
using BudgetManagement.Application.Common.Interfaces.IMiscMaster;
using Dapper;

namespace BudgetManagement.Infrastructure.Repositories.MiscMaster
{
    public class MiscMasterQueryRepository : IMiscMasterQueryRepository    
    {
        private readonly IDbConnection _dbConnection;

        public MiscMasterQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;

        }

        public async Task<(List<BudgetManagement.Domain.Entities.MiscMaster>, int)> GetAllMiscMasterAsync(int PageNumber, int PageSize, string? SearchTerm)
        {
            var query = $$"""
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*) 
                FROM [Budget].[MiscMaster] M
                WHERE M.IsDeleted = 0
                {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (M.Code LIKE @Search)")}}; 

                SELECT M.Id, M.MiscTypeId, M.Code, M.Description, M.SortOrder, M.IsActive, M.IsDeleted, 
                    M.CreatedBy, M.CreatedDate, M.CreatedByName, M.CreatedIP, M.ModifiedBy, M.ModifiedDate, 
                    M.ModifiedByName, M.ModifiedIP
                FROM Budget.MiscMaster M
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
            var miscMasterList = (await result.ReadAsync<BudgetManagement.Domain.Entities.MiscMaster>()).ToList();

            // Read the total count
            int totalCount = await result.ReadFirstAsync<int>();

            return (miscMasterList, totalCount);

        }


        public async Task<BudgetManagement.Domain.Entities.MiscMaster> GetByIdAsync(int id)
        {
            const string query = @" SELECT Id,MiscTypeId,Code,Description,SortOrder,IsActive  FROM Budget.MiscMaster          
             WHERE Id = @id AND IsDeleted = 0 ";
            return await _dbConnection.QueryFirstOrDefaultAsync<BudgetManagement.Domain.Entities.MiscMaster>(query, new { id }) ?? new BudgetManagement.Domain.Entities.MiscMaster();
        }


        public async Task<List<BudgetManagement.Domain.Entities.MiscMaster>> GetMiscMaster(string searchPattern, string miscTypeCode)
        {


            const string query = @"SELECT M.Id,M.Code ,M.Description  FROM Budget.MiscMaster M
            INNER JOIN [Budget].[MiscTypeMaster] MT ON MT.Id = M.MiscTypeId
                WHERE M.IsDeleted = 0 AND MT.IsDeleted = 0 AND M.IsActive = 1  AND MT.MiscTypeCode= @MiscTypeCode AND M.Code LIKE @SearchPattern  ";


            var parameters = new
            {
                SearchPattern = $"%{searchPattern ?? string.Empty}%",
                MiscTypeCode = miscTypeCode

            };

            var miscmaster = await _dbConnection.QueryAsync<BudgetManagement.Domain.Entities.MiscMaster>(query, parameters);
            return miscmaster.ToList();
        }

        public async Task<BudgetManagement.Domain.Entities.MiscMaster?> GetByMiscMasterCodeAsync(string name, int? id = null)
        {
            var query = """
                 SELECT * FROM Budget.MiscMaster
                 WHERE Code = @Name AND IsDeleted = 0 
                 """;

            var parameters = new DynamicParameters(new { Name = name });

            if (id is not null)
            {
                query += " AND Id != @Id";
                parameters.Add("Id", id);
            }

            return await _dbConnection.QueryFirstOrDefaultAsync<BudgetManagement.Domain.Entities.MiscMaster>(query, parameters);
        }

        public async Task<int> GetMaxSortOrderAsync()
        {
            var query = "SELECT ISNULL(MAX(SortOrder), 0) FROM Budget.MiscMaster WHERE IsDeleted = 0 ";
            return await _dbConnection.QueryFirstOrDefaultAsync<int>(query);
        }

        public async Task<bool> AlreadyExistsAsync(string code, int miscTypeId, int? id = null)
        {
            var query = @"SELECT COUNT(1) 
                        FROM Budget.MiscMaster 
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
            var query = "SELECT COUNT(1) FROM Budget.MiscMaster WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(query, new { Id = id });
            // True when the record is NOT found (honest to the method name). Validators use
            // !NotFoundAsync(id) so an existing row passes and a missing row fails "not found".
            return count == 0;
        }

        public async Task<bool> FKColumnValidation(int MiscMasterId)
        {
            var query = "SELECT COUNT(1) FROM Budget.MiscMaster WHERE Id = @Id AND IsDeleted = 0   ";

            var count = await _dbConnection.ExecuteScalarAsync<int>(query, new { Id = MiscMasterId });
            return count > 0;
        }
        public async Task<BudgetManagement.Domain.Entities.MiscMaster?> GetMiscMasterByName(string miscTypeCode, string miscTypeName)
        {


            const string sql = @"
                SELECT M.Id, M.Code, M.Description
                FROM Budget.MiscMaster AS M
                INNER JOIN Budget.MiscTypeMaster AS MT ON MT.Id = M.MiscTypeId
                WHERE M.IsDeleted = 0 AND M.IsActive = 1
                AND MT.IsDeleted = 0
                AND LOWER(MT.MiscTypeCode) = LOWER(@MiscTypeCode)
                AND LOWER(M.Code) = LOWER(@MiscTypeName);";


            var p = new { MiscTypeCode = miscTypeCode, MiscTypeName = miscTypeName };
            return await _dbConnection.QueryFirstOrDefaultAsync<BudgetManagement.Domain.Entities.MiscMaster>(sql, p);
        }      
        public async Task<BudgetManagement.Domain.Entities.MiscMaster?> GetByTypeAndCodeAsync(
            string miscTypeCode,
            string code,
            CancellationToken ct = default)
        {
            const string sql = @"
                SELECT m.*
                FROM Budget.MiscMaster m
                INNER JOIN Budget.MiscTypeMaster t ON t.Id = m.MiscTypeId
                WHERE t.MiscTypeCode = @MiscTypeCode AND m.Code = @Code";

            return await _dbConnection.QueryFirstOrDefaultAsync<BudgetManagement.Domain.Entities.MiscMaster>(sql, new
            {
                MiscTypeCode = miscTypeCode,
                Code = code
            });
        }


        public async Task<bool> SoftDeleteValidation(int id)
        {
            const string query = @"
                SELECT CASE WHEN
                    EXISTS (SELECT 1 FROM [Budget].[BudgetGroup] WHERE AllocationRuleId = @id AND IsDeleted = 0)
                    OR EXISTS (SELECT 1 FROM [Budget].[BudgetGroup] WHERE BudgetTypeId = @id AND IsDeleted = 0)
                    OR EXISTS (SELECT 1 FROM [Budget].[BudgetRequest] WHERE RequestTypeId = @id AND IsDeleted = 0)
                    OR EXISTS (SELECT 1 FROM [Budget].[BudgetRequest] WHERE RequestById = @id AND IsDeleted = 0)
                    OR EXISTS (SELECT 1 FROM [Budget].[BudgetRequest] WHERE RequestMonthId = @id AND IsDeleted = 0)
                    OR EXISTS (SELECT 1 FROM [Budget].[BudgetAllocation] WHERE AllocationTypeId = @id AND IsDeleted = 0)
                    OR EXISTS (SELECT 1 FROM [Budget].[BudgetAllocation] WHERE RequestById = @id AND IsDeleted = 0)
                    OR EXISTS (SELECT 1 FROM [Budget].[BudgetAllocation] WHERE RequestMonthId = @id AND IsDeleted = 0)
                THEN 1 ELSE 0 END;";

            var result = await _dbConnection.QueryFirstOrDefaultAsync<int>(query, new { id });
            return result == 1;
        }

        public async Task<bool> IsMiscMasterLinkedAsync(int id)
        {
            const string query = @"
                SELECT CASE WHEN
                    EXISTS (SELECT 1 FROM [Budget].[BudgetGroup] WHERE AllocationRuleId = @id AND IsDeleted = 0 AND IsActive = 1)
                    OR EXISTS (SELECT 1 FROM [Budget].[BudgetGroup] WHERE BudgetTypeId = @id AND IsDeleted = 0 AND IsActive = 1)
                    OR EXISTS (SELECT 1 FROM [Budget].[BudgetRequest] WHERE RequestTypeId = @id AND IsDeleted = 0 AND IsActive = 1)
                    OR EXISTS (SELECT 1 FROM [Budget].[BudgetRequest] WHERE RequestById = @id AND IsDeleted = 0 AND IsActive = 1)
                    OR EXISTS (SELECT 1 FROM [Budget].[BudgetRequest] WHERE RequestMonthId = @id AND IsDeleted = 0 AND IsActive = 1)
                    OR EXISTS (SELECT 1 FROM [Budget].[BudgetAllocation] WHERE AllocationTypeId = @id AND IsDeleted = 0 AND IsActive = 1)
                    OR EXISTS (SELECT 1 FROM [Budget].[BudgetAllocation] WHERE RequestById = @id AND IsDeleted = 0 AND IsActive = 1)
                    OR EXISTS (SELECT 1 FROM [Budget].[BudgetAllocation] WHERE RequestMonthId = @id AND IsDeleted = 0 AND IsActive = 1)
                THEN 1 ELSE 0 END;";

            var result = await _dbConnection.QueryFirstOrDefaultAsync<int>(query, new { id });
            return result == 1;
        }

    }
}