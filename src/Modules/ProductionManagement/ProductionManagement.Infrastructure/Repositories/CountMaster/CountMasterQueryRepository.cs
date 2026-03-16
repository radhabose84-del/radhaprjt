using System.Data;
using Contracts.Interfaces.Lookups.Inventory;
using Dapper;
using ProductionManagement.Application.Common.Interfaces.ICountMaster;
using ProductionManagement.Application.CountMaster.Dto;

namespace ProductionManagement.Infrastructure.Repositories.CountMaster
{
    public class CountMasterQueryRepository : ICountMasterQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IUOMLookup _uomLookup;

        public CountMasterQueryRepository(IDbConnection dbConnection, IUOMLookup uomLookup)
        {
            _dbConnection = dbConnection;
            _uomLookup = uomLookup;
        }

        public async Task<(List<CountMasterDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var offset = (pageNumber - 1) * pageSize;

            const string sql = @"
                DECLARE @TotalCount INT;

                SELECT @TotalCount = COUNT(*)
                FROM Production.CountMaster cm
                WHERE cm.IsDeleted = 0
                  AND (@SearchTerm IS NULL
                       OR cm.CountCode LIKE @SearchTerm
                       OR cm.CountDescription LIKE @SearchTerm
                       OR cm.ShortName LIKE @SearchTerm);

                SELECT
                    cm.Id, cm.CountCode, cm.CountValue, cm.ShortName,
                    cm.CountCategoryId, cm.CountTypeId,
                    cm.CountDescription, cm.UOMId,
                    cm.IsActive, cm.IsDeleted,
                    cm.CreatedBy, cm.CreatedDate, cm.CreatedByName, cm.CreatedIP,
                    cm.ModifiedBy, cm.ModifiedDate, cm.ModifiedByName, cm.ModifiedIP,
                    ct.Description AS CountTypeName,
                    cc.Description AS CountCategoryName
                FROM Production.CountMaster cm
                LEFT JOIN Production.MiscMaster ct  ON cm.CountTypeId     = ct.Id AND ct.IsDeleted  = 0
                LEFT JOIN Production.MiscMaster cc  ON cm.CountCategoryId = cc.Id AND cc.IsDeleted  = 0
                WHERE cm.IsDeleted = 0
                  AND (@SearchTerm IS NULL
                       OR cm.CountCode LIKE @SearchTerm
                       OR cm.CountDescription LIKE @SearchTerm
                       OR cm.ShortName LIKE @SearchTerm)
                ORDER BY cm.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;";

            var parameters = new
            {
                SearchTerm = string.IsNullOrWhiteSpace(searchTerm) ? (object?)null : $"%{searchTerm}%",
                Offset = offset,
                PageSize = pageSize
            };

            using var multi = await _dbConnection.QueryMultipleAsync(sql, parameters);
            var list = (await multi.ReadAsync<CountMasterDto>()).ToList();
            var totalCount = await multi.ReadFirstAsync<int>();

            if (list.Any())
            {
                var uomIds = list.Select(x => x.UOMId).Distinct();
                var uoms = await _uomLookup.GetByIdsAsync(uomIds);
                var uomDict = uoms.ToDictionary(x => x.Id, x => new { x.Code, x.UOMName });

                foreach (var item in list)
                {
                    if (uomDict.TryGetValue(item.UOMId, out var uom))
                    {
                        item.UOMCode = uom.Code;
                        item.UOMName = uom.UOMName;
                    }
                }
            }

            return (list, totalCount);
        }

        public async Task<CountMasterDto?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT
                    cm.Id, cm.CountCode, cm.CountValue, cm.ShortName,
                    cm.CountCategoryId, cm.CountTypeId,
                    cm.CountDescription, cm.UOMId,
                    cm.IsActive, cm.IsDeleted,
                    cm.CreatedBy, cm.CreatedDate, cm.CreatedByName, cm.CreatedIP,
                    cm.ModifiedBy, cm.ModifiedDate, cm.ModifiedByName, cm.ModifiedIP,
                    ct.Description AS CountTypeName,
                    cc.Description AS CountCategoryName
                FROM Production.CountMaster cm
                LEFT JOIN Production.MiscMaster ct  ON cm.CountTypeId     = ct.Id AND ct.IsDeleted  = 0
                LEFT JOIN Production.MiscMaster cc  ON cm.CountCategoryId = cc.Id AND cc.IsDeleted  = 0
                WHERE cm.Id = @Id AND cm.IsDeleted = 0";

            var dto = await _dbConnection.QueryFirstOrDefaultAsync<CountMasterDto>(sql, new { Id = id });

            if (dto != null)
            {
                var uoms = await _uomLookup.GetByIdsAsync(new[] { dto.UOMId });
                var uom = uoms.FirstOrDefault();
                if (uom != null)
                {
                    dto.UOMCode = uom.Code;
                    dto.UOMName = uom.UOMName;
                }
            }

            return dto;
        }

        public async Task<IReadOnlyList<CountMasterLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            const string sql = @"
                SELECT Id, CountCode, CountDescription
                FROM Production.CountMaster
                WHERE IsDeleted = 0 AND IsActive = 1
                  AND (CountCode LIKE @Term OR CountDescription LIKE @Term)
                ORDER BY CountDescription ASC";

            var result = await _dbConnection.QueryAsync<CountMasterLookupDto>(
                new CommandDefinition(sql, new { Term = $"%{term}%" }, cancellationToken: ct));
            return result.ToList();
        }

        public async Task<string> GetNextCountCodeAsync()
        {
            const string sql = @"
                SELECT ISNULL(MAX(CAST(CountCode AS INT)), 0) + 1
                FROM Production.CountMaster";

            var next = await _dbConnection.ExecuteScalarAsync<int>(sql);
            return next.ToString();
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Production.CountMaster
                WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<bool> CountTypeExistsAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Production.MiscMaster
                WHERE Id = @Id AND IsActive = 1 AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count > 0;
        }

        public async Task<bool> CountCategoryExistsAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Production.MiscMaster
                WHERE Id = @Id AND IsActive = 1 AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count > 0;
        }
    }
}
