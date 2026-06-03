using System.Data;
using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces.ILocation;
using Dapper;

namespace UserManagement.Infrastructure.Repositories.Location
{
    public class LocationQueryRepository : ILocationQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IIPAddressService _ipAddressService;

        public LocationQueryRepository(IDbConnection dbConnection, IIPAddressService ipAddressService)
        {
            _dbConnection = dbConnection;
            _ipAddressService = ipAddressService;
        }

        public async Task<UserManagement.Domain.Entities.Location?> GetLocationByIdAsync(int id)
        {
            const string sql = @"
                SELECT
                    Id,
                    Code,
                    LocationName,
                    Description,
                    IsActive,
                    CreatedBy,
                    CreatedDate AS CreatedAt,
                    CreatedByName,
                    CreatedIP,
                    ModifiedBy,
                    ModifiedDate AS ModifiedAt,
                    ModifiedByName,
                    ModifiedIP,
                    IsDeleted
                FROM AppData.Location
                WHERE Id = @Id AND IsDeleted = 0";

            return await _dbConnection.QueryFirstOrDefaultAsync<UserManagement.Domain.Entities.Location>(sql, new { Id = id });
        }

        public async Task<(List<UserManagement.Domain.Entities.Location>, int)> GetAllLocationAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var query = $$"""
                    DECLARE @TotalCount INT;

                    SELECT @TotalCount = COUNT(*)
                    FROM AppData.Location
                    WHERE IsDeleted = 0
                    {{(string.IsNullOrEmpty(searchTerm) ? "" : "AND (Code LIKE @Search OR LocationName LIKE @Search)")}};

                    SELECT
                        [Id],
                        [Code],
                        [LocationName],
                        [Description],
                        [IsActive],
                        [CreatedBy],
                        [CreatedDate] AS CreatedAt,
                        [CreatedByName],
                        [CreatedIP],
                        [ModifiedBy],
                        [ModifiedDate] AS ModifiedAt,
                        [ModifiedByName],
                        [ModifiedIP],
                        [IsDeleted]
                    FROM AppData.Location
                    WHERE IsDeleted = 0
                    {{(string.IsNullOrEmpty(searchTerm) ? "" : "AND (Code LIKE @Search OR LocationName LIKE @Search)")}}
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

            var result = await _dbConnection.QueryMultipleAsync(query, parameters);

            var locationList = (await result.ReadAsync<UserManagement.Domain.Entities.Location>()).ToList();
            var totalCount = await result.ReadFirstAsync<int>();

            return (locationList, totalCount);
        }

        public async Task<List<UserManagement.Domain.Entities.Location>> GetAllLocationAsync(string searchPattern)
        {
            const string query = @"
                    SELECT
                        [Id],
                        [Code],
                        [LocationName],
                        [Description],
                        [IsActive],
                        [CreatedBy],
                        [CreatedDate]  AS CreatedAt,
                        [CreatedByName],
                        [CreatedIP],
                        [ModifiedBy],
                        [ModifiedDate] AS ModifiedAt,
                        [ModifiedByName],
                        [ModifiedIP],
                        [IsDeleted]
                    FROM AppData.Location
                    WHERE
                        (Code LIKE @SearchPattern OR LocationName LIKE @SearchPattern)
                        AND IsDeleted = 0 AND IsActive = 1
                    ORDER BY Id DESC";

            var result = await _dbConnection.QueryAsync<UserManagement.Domain.Entities.Location>(
                query,
                new { SearchPattern = $"%{searchPattern}%" });

            return result.ToList();
        }

        public async Task<bool> AlreadyExistsAsync(string code, int? id = null)
        {
            var sql = @"
                SELECT COUNT(1)
                FROM AppData.Location
                WHERE Code = @Code AND IsDeleted = 0";

            if (id.HasValue && id.Value > 0)
                sql += " AND Id != @Id";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Code = code.Trim(), Id = id });
            return count > 0;
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM AppData.Location
                WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }
    }
}
