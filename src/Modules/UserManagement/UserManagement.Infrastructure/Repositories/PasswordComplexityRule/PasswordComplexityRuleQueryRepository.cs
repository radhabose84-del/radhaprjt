using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common.Interfaces.IPasswordComplexityRule;
using Core.Domain.Entities;
using Dapper;
using DnsClient.Internal;



namespace UserManagement.Infrastructure.Repositories.PasswordComplexityRule
{
    public class PasswordComplexityRuleQueryRepository : IPasswordComplexityRuleQueryRepository
    { 
          private readonly IDbConnection _dbConnection; 
    public  PasswordComplexityRuleQueryRepository(IDbConnection dbConnection)
    {
         _dbConnection = dbConnection;
    }

      //public async Task<List<Core.Domain.Entities.PasswordComplexityRule>>GetPasswordComplexityAsync( )
       public async Task<(List<Core.Domain.Entities.PasswordComplexityRule>,int)> GetPasswordComplexityAsync(int PageNumber, int PageSize, string? SearchTerm)
    {
          var query = $$"""
             DECLARE @TotalCount INT;
             SELECT @TotalCount = COUNT(*) 
               FROM AppSecurity.PasswordComplexityRule 
              WHERE IsDeleted = 0  
            {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (PwdComplexityRule LIKE @Search OR Id LIKE @Search)")}};
            SELECT Id,PwdComplexityRule
            ,IsActive
            ,CreatedBy
            ,CreatedAt
            ,CreatedByName
            ,CreatedIP
            ,ModifiedBy
            ,ModifiedAt
            ,ModifiedByName
            ,ModifiedIP
            ,IsDeleted
            FROM AppSecurity.PasswordComplexityRule WHERE IsDeleted = 0
            {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (PwdComplexityRule LIKE @Search OR Id LIKE @Search )")}}
            ORDER BY Id DESC              
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            SELECT @TotalCount AS TotalCount ;
            """;

           var parameters = new
                       {
                           Search = $"%{SearchTerm}%",
                           Offset = (PageNumber - 1) * PageSize,
                           PageSize
                       };

               var passwordComplexityRule = await _dbConnection.QueryMultipleAsync(query, parameters);
             var PasswordComplexityRulelist = (await passwordComplexityRule.ReadAsync<Core.Domain.Entities.PasswordComplexityRule>()).ToList();
             int totalCount = (await passwordComplexityRule.ReadFirstAsync<int>());
            return (PasswordComplexityRulelist, totalCount);      

      
    }

      public async Task<Core.Domain.Entities.PasswordComplexityRule> GetByIdAsync(int id)
    {
       

         const string query = @"SELECT * FROM AppSecurity.PasswordComplexityRule WHERE Id = @Id AND IsDeleted = 0 ORDER BY Id DESC";
            var passwordComplexity = await _dbConnection.QueryFirstOrDefaultAsync<Core.Domain.Entities.PasswordComplexityRule>(query, new { id });           
             if (passwordComplexity == null)
            {
             
               return null;            
               
            }
            return passwordComplexity;
        }       
         public async Task<List<Core.Domain.Entities.PasswordComplexityRule>>  GetpwdautocompleteAsync(string searchTerm = null)
        {


           const string query = @" 
           SELECT Id,PwdComplexityRule
            ,IsActive
            ,CreatedBy
            ,CreatedAt
            ,CreatedByName
            ,CreatedIP
            ,ModifiedBy
            ,ModifiedAt
            ,ModifiedByName
            ,ModifiedIP
            ,IsDeleted
            FROM AppSecurity.PasswordComplexityRule WHERE  PwdComplexityRule LIKE @searchTerm OR Id LIKE @searchTerm AND IsDeleted = 0
            ORDER BY ID DESC";
              var parameters = new 
              { 
                  searchTerm = $"%{searchTerm ?? string.Empty}%"
              };

               var Pwdrule = await _dbConnection.QueryAsync<Core.Domain.Entities.PasswordComplexityRule>(query, parameters);
            return Pwdrule.ToList();   
        }
    }

   
}