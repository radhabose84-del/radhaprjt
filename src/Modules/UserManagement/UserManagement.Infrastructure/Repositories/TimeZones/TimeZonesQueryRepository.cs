using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common.Interfaces.ITimeZones;
using Dapper;

namespace UserManagement.Infrastructure.Repositories.TimeZones
{
    public class TimeZonesQueryRepository : ITimeZonesQueryRepository
    {
        private readonly IDbConnection _dbConnection; 
        public TimeZonesQueryRepository(IDbConnection dbConnection)
        {
             _dbConnection = dbConnection;
        }

        public async Task<(List<Core.Domain.Entities.TimeZones>, int)> GetAllTimeZonesAsync(int PageNumber, int PageSize, string? SearchTerm)
        {
             var query = $$"""
             DECLARE @TotalCount INT;
             SELECT @TotalCount = COUNT(*) 
               FROM AppData.TimeZones
              WHERE IsDeleted = 0
            {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (Name LIKE @Search OR Code LIKE @Search)")}};

                SELECT 
                Id, 
                Code,
                Name,
                Location,
                Offset,
                IsActive,
                CreatedByName,
                CreatedAt,
                CreatedIP,
                ModifiedByName,
                ModifiedAt,
                ModifiedIP
            FROM AppData.TimeZones 
            WHERE 
            IsDeleted = 0
                {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (Name LIKE @Search OR Code LIKE @Search )")}}
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

             var timezonesgroup = await _dbConnection.QueryMultipleAsync(query, parameters);
             var timeZonesgrouplist = (await timezonesgroup.ReadAsync<Core.Domain.Entities.TimeZones>()).ToList();
             int totalCount = (await timezonesgroup.ReadFirstAsync<int>());
             return (timeZonesgrouplist, totalCount);
        }

        public async Task<Core.Domain.Entities.TimeZones> GetByIdAsync(int Id)
        {
              const string query = @"
                    SELECT * 
                    FROM AppData.TimeZones 
                    WHERE Id = @Id AND IsDeleted = 0";
                    var timeZonesGroup = await _dbConnection.QueryFirstOrDefaultAsync<Core.Domain.Entities.TimeZones>(query, new { Id });
                    return timeZonesGroup;
        }

        public async Task<List<Core.Domain.Entities.TimeZones>> GetByTimeZonesNameAsync(string searchPattern)
        {
            searchPattern = searchPattern ?? string.Empty; // Prevent null issues

            const string query = @"
            SELECT Id, Name 
            FROM AppData.TimeZones
            WHERE IsDeleted = 0 
            AND Name LIKE @SearchPattern";  
            var parameters = new 
            { 
            SearchPattern = $"%{searchPattern}%" 
            };

            var timeZonesGroups = await _dbConnection.QueryAsync<Core.Domain.Entities.TimeZones>(query, parameters);
            return timeZonesGroups.ToList();    
        }
    }
}