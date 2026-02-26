using System.Data;
using FAM.Application.Common.Interfaces;
using FAM.Application.Common.Interfaces.ILocation;
using FAM.Domain.Entities;
using Dapper;
using FAM.Application.Location.Queries.GetLocations;

namespace FAM.Infrastructure.Repositories.Locations
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
        // public async Task<(List<Location>, int)> GetAllLocationAsync(int PageNumber, int PageSize, string? SearchTerm)
        // {
        //     // var UnitId = _ipAddressService.GetUnitId();
        //     var query = $$"""
        //      DECLARE @TotalCount INT;
        //      SELECT @TotalCount = COUNT(*) 
        //        FROM FixedAsset.Location 
        //       WHERE IsDeleted = 0 
        //     {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (Code LIKE @Search OR LocationName LIKE @Search)")}};

        //         SELECT 
        //         Id, 
        //         Code,
        //         LocationName,
        //         Description,
        //         SortOrder,
        //         UnitId,
        //         DepartmentId,
        //         IsActive,
        //         CreatedDate,
        //         CreatedByName
        //     FROM FixedAsset.Location 
        //     WHERE 
        //     IsDeleted = 0 
        //         {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (Code LIKE @Search OR LocationName LIKE @Search )")}}
        //         ORDER BY Id DESC
        //         OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

        //         SELECT @TotalCount AS TotalCount;
        //     """;


        //     var parameters = new
        //     {
        //         Search = $"%{SearchTerm}%",
        //         Offset = (PageNumber - 1) * PageSize,
        //         PageSize
        //     };

        //     var location = await _dbConnection.QueryMultipleAsync(query, parameters);
        //     var locationlist = (await location.ReadAsync<Location>()).ToList();
        //     int totalCount = (await location.ReadFirstAsync<int>());
        //     return (locationlist, totalCount);
        // }

        public async Task<(List<LocationDto>, int)> GetAllLocationListAsync(int PageNumber, int PageSize, string SearchTerm)
        {
            var unitId = _ipAddressService.GetUnitId();

            var sql = $$"""
                DECLARE @TotalCount INT;

                SELECT @TotalCount = COUNT(*)
                FROM [FixedAsset].[Location] L
                WHERE L.IsDeleted = 0
                  AND L.UnitId = @UnitId
                  {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (L.Code LIKE @Search OR L.LocationName LIKE @Search)")}} ;

                SELECT
                    L.Id,
                    L.Code,
                    L.LocationName,
                    L.Description,
                    L.SortOrder,
                    L.UnitId,
                    L.DepartmentId,
                    CAST(NULL AS NVARCHAR(200)) AS DepartmentName,  -- ✅ will be filled by lookup in handler
                    L.IsActive,
                    L.CreatedDate,
                    L.CreatedByName
                FROM [FixedAsset].[Location] L
                WHERE L.IsDeleted = 0
                  AND L.UnitId = @UnitId
                  {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (L.Code LIKE @Search OR L.LocationName LIKE @Search)")}}
                ORDER BY L.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;
            """;

            var parameters = new
            {
                Search = $"%{SearchTerm}%",
                Offset = (PageNumber - 1) * PageSize,
                PageSize,
                UnitId = unitId
            };

            using var multi = await _dbConnection.QueryMultipleAsync(sql, parameters);

            var list = (await multi.ReadAsync<LocationDto>()).ToList();
            var total = await multi.ReadFirstAsync<int>();

            return (list, total);
        }

        public async Task<Location> GetByIdAsync(int id)
        {
            const string query = "SELECT * FROM FixedAsset.Location WHERE Id = @Id AND IsDeleted = 0";
            return await _dbConnection.QueryFirstOrDefaultAsync<Location>(query, new { id });
        }

        public async Task<Location> GetByLocationNameAsync(string name, int DepartmentId, int UnitId, int? id = null)
        {
            var query = @"
            SELECT
                L.Id,
                L.Code,
                L.LocationName,
                L.Description,
                L.SortOrder,
                L.UnitId,
                L.DepartmentId,
                L.IsActive,
                L.IsDeleted,
                L.CreatedBy,
                L.CreatedDate,
                L.CreatedByName,
                L.CreatedIP,
                L.ModifiedBy,
                L.ModifiedDate,
                L.ModifiedByName,
                L.ModifiedIP
            FROM FixedAsset.Location L
            WHERE L.LocationName = @LocationName
              AND L.IsDeleted = 0
              AND L.DepartmentId = @DepartmentId
              AND L.UnitId = @UnitId
        ";

            return await _dbConnection.QueryFirstOrDefaultAsync<Location>(query, new { LocationName = name, DepartmentId = DepartmentId, UnitId = UnitId });
            // var query = """
            //      SELECT * FROM FixedAsset.Location
            //      WHERE LocationName = @LocationName AND IsDeleted = 0
            //      """;

            // var parameters = new DynamicParameters(new { LocationName = name });

            // if (id is not null)
            // {
            //     query += " AND Id != @Id";
            //     parameters.Add("Id", id);
            // }

            // return await _dbConnection.QueryFirstOrDefaultAsync<Location>(query, parameters);
        }
        public async Task<List<Location>> GetLocation(string searchPattern = null)
        {
            const string query = @"
                SELECT Id, LocationName 
                FROM FixedAsset.Location 
                WHERE IsDeleted = 0 AND IsActive = 1 AND LocationName LIKE @SearchPattern";

            var locations = await _dbConnection.QueryAsync<Location>(query, new { SearchPattern = $"%{searchPattern}%" });
            return locations.ToList();
        }

        public async Task<bool> IsLinkedWithSubLocationsAsync(int locationId)
        {
            const string query = @"
        SELECT TOP 1 1
        FROM [FixedAsset].[FixedAsset].[SubLocation]
        WHERE IsDeleted = 0 AND LocationId = @locationId;
        ";

            var result = await _dbConnection.QueryFirstOrDefaultAsync<int?>(query, new { locationId });
            return result.HasValue;
        }
    }
}