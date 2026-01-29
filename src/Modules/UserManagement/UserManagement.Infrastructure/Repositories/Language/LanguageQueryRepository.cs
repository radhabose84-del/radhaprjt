using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common.Interfaces.ILanguage;
using Dapper;

namespace UserManagement.Infrastructure.Repositories.Language
{
    public class LanguageQueryRepository : ILanguageQuery
    {
        private readonly IDbConnection _dbConnection;
        public LanguageQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }
        public async Task<(List<Core.Domain.Entities.Language>,int)> GetAllLanguageAsync(int PageNumber, int PageSize, string? SearchTerm)
        {
             var query = $$"""

              DECLARE @TotalCount INT;
             SELECT @TotalCount = COUNT(*) 
               FROM AppData.Language 
              WHERE IsDeleted = 0
            {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (Code LIKE @Search OR Name LIKE @Search)")}};

                SELECT 
                Id, 
                Code,
                Name,
                IsActive,
                CreatedAt,
                CreatedByName,
                CreatedIP,
                ModifiedAt,
                ModifiedByName,
                ModifiedIP
            FROM AppData.Language WHERE IsDeleted = 0
                {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (Code LIKE @Search OR Name LIKE @Search )")}}
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

             var language = await _dbConnection.QueryMultipleAsync(query, parameters);
             var languagelist = (await language.ReadAsync<Core.Domain.Entities.Language>()).ToList();
             int totalCount = (await language.ReadFirstAsync<int>());
             
           return (languagelist, totalCount);
        }

        public async Task<Core.Domain.Entities.Language> GetByIdAsync(int id)
        {
            const string query = "SELECT * FROM AppData.Language WHERE Id = @Id AND IsDeleted = 0";
            return await _dbConnection.QueryFirstOrDefaultAsync<Core.Domain.Entities.Language>(query, new { id });
        }

        public async Task<Core.Domain.Entities.Language?> GetByLanguagenameAsync(string name, int? id = null)
        {
             if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("LanguageName cannot be null or empty.", nameof(name));
            }


             var query = """
                 SELECT * FROM AppData.Language 
                 WHERE Name = @Name AND IsDeleted = 0
                 """;

             var parameters = new DynamicParameters(new { Name = name });

             if (id is not null)
             {
                 query += " AND Id != @Id";
                 parameters.Add("Id", id);
             }

            return await _dbConnection.QueryFirstOrDefaultAsync<Core.Domain.Entities.Language>(query, parameters);
            
        }

        public async Task<List<Core.Domain.Entities.Language>> GetLanguage(string searchPattern=null)
        {

            const string query = @"
                SELECT Id, Name 
                FROM AppData.Language 
                WHERE IsDeleted = 0 AND Name LIKE @SearchPattern";
                
            var languages = await _dbConnection.QueryAsync<Core.Domain.Entities.Language>(query, new { SearchPattern = $"%{searchPattern}%" });
            return languages.ToList();
        }
    }
}