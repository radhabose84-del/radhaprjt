using System.Data;
using Dapper;
using ProductionManagement.Application.Common.Interfaces.IMiscTypeMaster;
using ProductionManagement.Application.MiscTypeMaster.Dto;

namespace ProductionManagement.Infrastructure.Repositories.MiscTypeMaster
{
    public class MiscTypeMasterQueryRepository : IMiscTypeMasterQueryRepository
    {
        private readonly IDbConnection _dbConnection;

        public MiscTypeMasterQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<(List<MiscTypeMasterDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var offset = (pageNumber - 1) * pageSize;

            var sql = @"
                SELECT COUNT(*)
                FROM Production.MiscTypeMaster
                WHERE IsDeleted = 0
                  AND (@SearchTerm IS NULL OR MiscTypeCode LIKE '%' + @SearchTerm + '%' OR Description LIKE '%' + @SearchTerm + '%');

                SELECT
                    Id, MiscTypeCode, Description,
                    IsActive, IsDeleted,
                    CreatedBy, CreatedDate, CreatedByName, CreatedIP,
                    ModifiedBy, ModifiedDate, ModifiedByName, ModifiedIP
                FROM Production.MiscTypeMaster
                WHERE IsDeleted = 0
                  AND (@SearchTerm IS NULL OR MiscTypeCode LIKE '%' + @SearchTerm + '%' OR Description LIKE '%' + @SearchTerm + '%')
                ORDER BY Id
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

            using var multi = await _dbConnection.QueryMultipleAsync(sql, new
            {
                SearchTerm = string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm,
                Offset = offset,
                PageSize = pageSize
            });

            var totalCount = await multi.ReadFirstAsync<int>();
            var data = (await multi.ReadAsync<MiscTypeMasterDto>()).ToList();

            return (data, totalCount);
        }

        public async Task<MiscTypeMasterDto?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT
                    Id, MiscTypeCode, Description,
                    IsActive, IsDeleted,
                    CreatedBy, CreatedDate, CreatedByName, CreatedIP,
                    ModifiedBy, ModifiedDate, ModifiedByName, ModifiedIP
                FROM Production.MiscTypeMaster
                WHERE Id = @Id AND IsDeleted = 0;";

            return await _dbConnection.QueryFirstOrDefaultAsync<MiscTypeMasterDto>(sql, new { Id = id });
        }

        public async Task<IReadOnlyList<MiscTypeMasterLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            const string sql = @"
                SELECT Id, MiscTypeCode, Description
                FROM Production.MiscTypeMaster
                WHERE IsActive = 1 AND IsDeleted = 0
                  AND (@Term = '' OR MiscTypeCode LIKE '%' + @Term + '%' OR Description LIKE '%' + @Term + '%')
                ORDER BY Description ASC;";

            var result = await _dbConnection.QueryAsync<MiscTypeMasterLookupDto>(sql, new { Term = term });
            return result.ToList();
        }

        public async Task<bool> AlreadyExistsAsync(string miscTypeCode, int? id = null)
        {
            const string sql = @"
                SELECT COUNT(1) FROM Production.MiscTypeMaster
                WHERE MiscTypeCode = @MiscTypeCode AND IsDeleted = 0
                  AND (@Id IS NULL OR Id <> @Id);";

            var count = await _dbConnection.QueryFirstAsync<int>(sql, new { MiscTypeCode = miscTypeCode, Id = id });
            return count > 0;
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1) FROM Production.MiscTypeMaster
                WHERE Id = @Id AND IsDeleted = 0;";

            var count = await _dbConnection.QueryFirstAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<bool> SoftDeleteValidationAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1) FROM Production.MiscMaster
                WHERE MiscTypeId = @Id AND IsDeleted = 0;";

            var count = await _dbConnection.QueryFirstAsync<int>(sql, new { Id = id });
            return count > 0;
        }

        public async Task<bool> IsMiscTypeMasterLinkedAsync(int id)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM [Production].[MiscMaster]
                    WHERE MiscTypeId = @Id AND IsDeleted = 0 AND IsActive = 1
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = id });
        }
    }
}
