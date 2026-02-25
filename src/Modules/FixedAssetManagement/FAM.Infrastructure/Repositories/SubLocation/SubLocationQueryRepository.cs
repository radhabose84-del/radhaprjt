#nullable disable
using System.Data;
using FAM.Application.Common.Interfaces.ISubLocation;
using Dapper;

namespace FAM.Infrastructure.Repositories.SubLocation
{
    public class SubLocationQueryRepository : ISubLocationQueryRepository
    {
        private readonly IDbConnection _dbConnection;

        public SubLocationQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }
        public async Task<(List<FAM.Domain.Entities.SubLocation>, int)> GetAllSubLocationAsync(int PageNumber, int PageSize, string SearchTerm)
        {
            var query = $$"""
             DECLARE @TotalCount INT;
             SELECT @TotalCount = COUNT(*)
               FROM FixedAsset.SubLocation 
              WHERE IsDeleted = 0
            {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (Code LIKE @Search OR SubLocationName LIKE @Search)")}};

            SELECT 
                Id, 
                Code,
                SubLocationName,
                Description,
                LocationId,
                UnitId,
                DepartmentId,
                IsActive,
                CreatedDate,
                CreatedByName
            FROM FixedAsset.SubLocation 
            WHERE 
            IsDeleted = 0
                {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (Code LIKE @Search OR SubLocationName LIKE @Search )")}}
                ORDER BY Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;
            """;


            var parameters = new
            {
                Search = $"%{SearchTerm}%",
                Offset = (PageNumber - 1) * PageSize,
                PageSize

            };

            var sublocation = await _dbConnection.QueryMultipleAsync(query, parameters);
            var sublocationlist = (await sublocation.ReadAsync<FAM.Domain.Entities.SubLocation>()).ToList();
            int totalCount = (await sublocation.ReadFirstAsync<int>());
            return (sublocationlist, totalCount);
        }

        public async Task<FAM.Domain.Entities.SubLocation> GetByIdAsync(int id)
        {
            const string query = "SELECT * FROM FixedAsset.SubLocation WHERE Id = @Id AND IsDeleted = 0";
            return await _dbConnection.QueryFirstOrDefaultAsync<FAM.Domain.Entities.SubLocation>(query, new { id });
        }

        public async Task<FAM.Domain.Entities.SubLocation> GetBySubLocationNameAsync(string name, int DepartmentId, int LocationId, int UnitId, int? id = null)
        {
            var query = @"
            SELECT
                S.Id,
                S.Code,
                S.SubLocationName,
                S.Description,
                S.UnitId,
                S.DepartmentId,
                S.LocationId,
                S.IsActive,
                S.IsDeleted,
                S.CreatedBy,
                S.CreatedDate,
                S.CreatedByName,
                S.CreatedIP,
                S.ModifiedBy,
                S.ModifiedDate,
                S.ModifiedByName,
                S.ModifiedIP
            FROM FixedAsset.SubLocation S
            JOIN FixedAsset.Location L ON L.Id = S.LocationId
            WHERE S.SubLocationName = @SubLocationName
              AND S.IsDeleted = 0
              AND S.DepartmentId = @DepartmentId
              AND S.LocationId = @LocationId
              AND S.UnitId = @UnitId
        ";

            return await _dbConnection.QueryFirstOrDefaultAsync<FAM.Domain.Entities.SubLocation>(query, new { SubLocationName = name, DepartmentId = DepartmentId, LocationId = LocationId, UnitId = UnitId });           
        }

        public async Task<List<FAM.Domain.Entities.SubLocation>> GetSubLocation(string searchPattern)
        {
            const string query = @"
                SELECT Id, SubLocationName 
                FROM FixedAsset.SubLocation 
                WHERE IsDeleted = 0 AND SubLocationName LIKE @SearchPattern";

            var locations = await _dbConnection.QueryAsync<FAM.Domain.Entities.SubLocation>(query, new { SearchPattern = $"%{searchPattern}%" });
            return locations.ToList();
        }

        public async Task<bool> IsParentLocationActiveAsync(int locationId)
        {
            const string query = @"
             SELECT TOP 1 IsActive
             FROM FixedAsset.Location
             WHERE Id = @LocationId AND IsDeleted = 0";

            var isActiveValue = await _dbConnection
                .QueryFirstOrDefaultAsync<int?>(query, new { LocationId = locationId });
            if (!isActiveValue.HasValue)
                return false;

            return isActiveValue.Value == 1;
        }

        public async Task<FAM.Domain.Entities.SubLocation> GetBySubLocationCodeAsync(string code, int DepartmentId, int LocationId, int UnitId, int? id = null)
        {
            const string query = @"
          SELECT TOP 1 *
          FROM FixedAsset.SubLocation
          WHERE Code = @Code
          AND IsDeleted = 0
          AND DepartmentId = @DepartmentId
          AND LocationId = @LocationId
          AND UnitId = @UnitId
    ";

            return await _dbConnection.QueryFirstOrDefaultAsync<FAM.Domain.Entities.SubLocation>(
                query,
                new { Code = (code ?? string.Empty).Trim(), DepartmentId, LocationId, UnitId }
            );
        }

        public async Task<bool> IsSubLocationLinkedAsync(int id)
        {
            const string query = @"
        SELECT TOP 1 1
        FROM [FixedAsset].[FixedAsset].[AssetLocation]
        WHERE SubLocationId = @id;
        ";

            var exists = await _dbConnection.QueryFirstOrDefaultAsync<int?>(query, new { id });
            return exists.HasValue;
        }
    }
}