using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.Application.Workflow.Common.Interfaces.IWorkflowType;
using BackgroundService.Domain.Entities.Workflow;
using Dapper;
using Microsoft.Extensions.DependencyInjection;

namespace BackgroundService.Infrastructure.Repositories.Workflow.WorkflowTypes
{
    public class WorkflowTypeQueryRepository : IWorkflowTypeQuery
    {
        private readonly IDbConnection _dbConnection;
        public WorkflowTypeQueryRepository([FromKeyedServices("Notification")] IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }
        public async Task<bool> AlreadyExistsAsync(int MenuId,int ModuleId, int? id = null)
        {
            var query = "SELECT COUNT(1) FROM [AppData].[WorkflowType] WHERE MenuId = @MenuId AND ModuleId = @ModuleId AND IsDeleted = 0";
            var parameters = new DynamicParameters(new { MenuId,ModuleId });

            if (id is not null)
            {
                query += " AND Id != @Id";
                parameters.Add("Id", id);
            }
            var count = await _dbConnection.ExecuteScalarAsync<int>(query, parameters);
            return count > 0;
        }

        public async Task<(List<WorkflowType>, int)> GetAllWorkflowTypeAsync(int PageNumber, int PageSize, string? SearchTerm)
        {
               var query = $$"""
             DECLARE @TotalCount INT;
             SELECT @TotalCount = COUNT(*) 
               FROM [AppData].[WorkflowType]
              WHERE IsDeleted = 0
            {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (MenuId LIKE @Search)")}};

                SELECT 
                Id, 
                ModuleId,
                MenuId,
                HasLine,
                IsMultiselect,
                IsActive,CreatedDate,CreatedBy,CreatedByName,ModifiedBy,ModifiedDate,ModifiedByName
            FROM [AppData].[WorkflowType]
            WHERE 
            IsDeleted = 0
                {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (MenuId LIKE @Search )")}}
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

            var WorkflowType = await _dbConnection.QueryMultipleAsync(query, parameters);
            var WorkflowTypelist = (await WorkflowType.ReadAsync<WorkflowType>()).ToList();
            int totalCount = await WorkflowType.ReadFirstAsync<int>();

            return (WorkflowTypelist, totalCount);
        }

        public async Task<WorkflowType> GetWorkflowByName(int MenuId)
        {
            const string query = @"
                SELECT Id, MenuId 
                FROM [AppData].[WorkflowType] 
                WHERE IsDeleted = 0 AND IsActive=1 AND MenuId= @MenuId";
                
            var WorkflowType = await _dbConnection.QueryAsync<WorkflowType>(query, new { MenuId });
            return WorkflowType.FirstOrDefault();
        }

        public async Task<List<WorkflowType>> GetWorkflowTypeAutoComplete(string searchPattern)
        {
              const string query = @"
                SELECT Id, MenuId, TransactionTypeId, IsMultiselect
                FROM [AppData].[WorkflowType]
                WHERE IsDeleted = 0 AND IsActive=1 AND MenuId LIKE @SearchPattern";
                
            var WorkflowType = await _dbConnection.QueryAsync<WorkflowType>(query, new { SearchPattern = $"%{searchPattern}%" });
            return WorkflowType.ToList();
        }

        public async Task<bool> NotFoundAsync(int id)
        {
             var query = "SELECT COUNT(1) FROM [AppData].[WorkflowType]  WHERE Id = @Id AND IsDeleted = 0";
             
                var count = await _dbConnection.ExecuteScalarAsync<int>(query, new { Id = id });
                return count > 0;
        }
    }
}