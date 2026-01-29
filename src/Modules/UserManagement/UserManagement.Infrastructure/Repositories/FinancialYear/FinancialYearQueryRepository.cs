using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common.Interfaces.IFinancialYear;
using Dapper;

namespace UserManagement.Infrastructure.Repositories.FinancialYear
{
    public class FinancialYearQueryRepository : IFinancialYearQueryRepository
    {        
          private readonly IDbConnection _dbConnection; 

    public  FinancialYearQueryRepository(IDbConnection dbConnection)
    {
         _dbConnection = dbConnection;
    }

   
            public async Task<(List<Core.Domain.Entities.FinancialYear>, int)> GetAllFinancialYearAsync(int PageNumber, int PageSize, string? SearchTerm)
        {
            var query = $$"""
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*) 
                FROM AppData.FinancialYear 
                WHERE IsDeleted = 0  
                {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (FinYearName LIKE @Search  OR Id LIKE @Search)")}};
                
                SELECT Id, StartYear, StartDate, EndDate, FinYearName, IsActive,
                    CreatedBy, CreatedAt, CreatedByName, CreatedIP,
                    ModifiedBy, ModifiedAt, ModifiedByName, ModifiedIP, IsDeleted
                FROM AppData.FinancialYear 
                WHERE IsDeleted = 0
                {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (FinYearName LIKE @Search   OR Id LIKE @Search)")}}
                ORDER BY Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;
            """;

            var parameters = new
            {
                Search = string.IsNullOrEmpty(SearchTerm) ? null : $"%{SearchTerm}%",
                Offset = (PageNumber - 1) * PageSize,
                PageSize
            };

            var financialYearsResult = await _dbConnection.QueryMultipleAsync(query, parameters);
            var financialYears = (await financialYearsResult.ReadAsync<Core.Domain.Entities.FinancialYear>()).ToList();
            int totalCount = await financialYearsResult.ReadFirstAsync<int>();

            return (financialYears, totalCount);
        }
        public async Task<Core.Domain.Entities.FinancialYear>GetByIdAsync(int id)
        {               
                const string query = @"SELECT * FROM AppData.FinancialYear WHERE Id = @Id AND   IsDeleted = 0  ORDER BY ID DESC";
                return await _dbConnection.QueryFirstOrDefaultAsync<Core.Domain.Entities.FinancialYear>(query, new { id });
        } 
       public async Task<List<Core.Domain.Entities.FinancialYear>> GetAllFinancialAutoCompleteSearchAsync(string searchTerm)
            {
                const string query = @"
                    SELECT Id, StartYear, StartDate, EndDate, FinYearName, IsActive, CreatedBy, CreatedAt, 
                        CreatedByName, CreatedIP, ModifiedBy, ModifiedAt, ModifiedByName, ModifiedIP, IsDeleted
                    FROM AppData.FinancialYear
                    WHERE (StartYear LIKE @SearchTerm OR Id LIKE @SearchTerm) 
                    AND IsDeleted = 0
                    ORDER BY Id DESC";

                var parameters = new 
                { 
                    SearchTerm = $"%{searchTerm ?? string.Empty}%"
                };

                var financialYears = await _dbConnection.QueryAsync<Core.Domain.Entities.FinancialYear>(query, parameters);
                return financialYears.ToList();
            }


          




    }
}