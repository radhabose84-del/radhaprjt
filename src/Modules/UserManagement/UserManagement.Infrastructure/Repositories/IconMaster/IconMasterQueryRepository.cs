using System.Data;
using Dapper;
using UserManagement.Application.Common.Interfaces.IIconMaster;

namespace UserManagement.Infrastructure.Repositories.IconMaster
{
    public class IconMasterQueryRepository : IIconMasterQueryRepository
    {
        private readonly IDbConnection _dbConnection;

        public IconMasterQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<(List<UserManagement.Domain.Entities.IconMaster>, int)> GetAllIconMasterAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var query = $$"""
             DECLARE @TotalCount INT;
             SELECT @TotalCount = COUNT(*)
               FROM AppData.IconMaster
              WHERE IsDeleted = 0
            {{(string.IsNullOrEmpty(searchTerm) ? "" : "AND (Keyword LIKE @Search OR IconName LIKE @Search OR IconLibrary LIKE @Search)")}};

                SELECT
                    Id,
                    Keyword,
                    IconName,
                    IconLibrary,
                    Size,
                    Style,
                    IsActive,
                    CreatedBy,
                    CreatedAt,
                    CreatedByName,
                    CreatedIP,
                    ModifiedBy,
                    ModifiedAt,
                    ModifiedByName,
                    ModifiedIP
                FROM AppData.IconMaster
                WHERE IsDeleted = 0
                {{(string.IsNullOrEmpty(searchTerm) ? "" : "AND (Keyword LIKE @Search OR IconName LIKE @Search OR IconLibrary LIKE @Search)")}}
                ORDER BY Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;
            """;

            var parameters = new
            {
                Search = $"%{searchTerm}%",
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            var multi = await _dbConnection.QueryMultipleAsync(query, parameters);
            var list = (await multi.ReadAsync<UserManagement.Domain.Entities.IconMaster>()).ToList();
            int totalCount = await multi.ReadFirstAsync<int>();
            return (list, totalCount);
        }

        public async Task<UserManagement.Domain.Entities.IconMaster?> GetByIdAsync(int id)
        {
            const string query = @"
                SELECT *
                FROM AppData.IconMaster
                WHERE Id = @Id AND IsDeleted = 0";
            return await _dbConnection.QueryFirstOrDefaultAsync<UserManagement.Domain.Entities.IconMaster>(query, new { Id = id });
        }

        public async Task<List<UserManagement.Domain.Entities.IconMaster>> GetByKeywordAsync(string searchPattern)
        {
            searchPattern ??= string.Empty;

            const string query = @"
                SELECT Id, Keyword, IconName, IconLibrary, Size, Style
                FROM AppData.IconMaster
                WHERE IsDeleted = 0
                  AND (Keyword LIKE @SearchPattern OR IconName LIKE @SearchPattern)
                ORDER BY Keyword ASC";

            var parameters = new { SearchPattern = $"%{searchPattern}%" };
            var list = await _dbConnection.QueryAsync<UserManagement.Domain.Entities.IconMaster>(query, parameters);
            return list.ToList();
        }
    }
}
