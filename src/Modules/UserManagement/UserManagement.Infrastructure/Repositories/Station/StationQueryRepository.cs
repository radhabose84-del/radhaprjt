using System.Data;
using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces.IStation;
using Dapper;

namespace UserManagement.Infrastructure.Repositories.Station
{
    public class StationQueryRepository : IStationQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IIPAddressService _ipAddressService;

        public StationQueryRepository(IDbConnection dbConnection, IIPAddressService ipAddressService)
        {
            _dbConnection = dbConnection;
            _ipAddressService = ipAddressService;
        }

        public async Task<UserManagement.Domain.Entities.Station?> GetStationByIdAsync(int id)
        {
            const string sql = @"
                SELECT
                    Id,
                    Code,
                    StationName,
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
                FROM AppData.Station
                WHERE Id = @Id AND IsDeleted = 0";

            return await _dbConnection.QueryFirstOrDefaultAsync<UserManagement.Domain.Entities.Station>(sql, new { Id = id });
        }

        public async Task<(List<UserManagement.Domain.Entities.Station>, int)> GetAllStationAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var query = $$"""
                    DECLARE @TotalCount INT;

                    SELECT @TotalCount = COUNT(*)
                    FROM AppData.Station
                    WHERE IsDeleted = 0
                    {{(string.IsNullOrEmpty(searchTerm) ? "" : "AND (Code LIKE @Search OR StationName LIKE @Search)")}};

                    SELECT
                        [Id],
                        [Code],
                        [StationName],
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
                    FROM AppData.Station
                    WHERE IsDeleted = 0
                    {{(string.IsNullOrEmpty(searchTerm) ? "" : "AND (Code LIKE @Search OR StationName LIKE @Search)")}}
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

            var stationList = (await result.ReadAsync<UserManagement.Domain.Entities.Station>()).ToList();
            var totalCount = await result.ReadFirstAsync<int>();

            return (stationList, totalCount);
        }

        public async Task<List<UserManagement.Domain.Entities.Station>> GetAllStationAsync(string searchPattern)
        {
            const string query = @"
                    SELECT
                        [Id],
                        [Code],
                        [StationName],
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
                    FROM AppData.Station
                    WHERE
                        (Code LIKE @SearchPattern OR StationName LIKE @SearchPattern)
                        AND IsDeleted = 0 AND IsActive = 1
                    ORDER BY Id DESC";

            var result = await _dbConnection.QueryAsync<UserManagement.Domain.Entities.Station>(
                query,
                new { SearchPattern = $"%{searchPattern}%" });

            return result.ToList();
        }

        public async Task<bool> AlreadyExistsAsync(string code, int? id = null)
        {
            var sql = @"
                SELECT COUNT(1)
                FROM AppData.Station
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
                FROM AppData.Station
                WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }
    }
}
