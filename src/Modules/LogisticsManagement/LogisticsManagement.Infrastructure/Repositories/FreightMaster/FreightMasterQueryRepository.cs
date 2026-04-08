using System.Data;
using Dapper;
using LogisticsManagement.Application.Common.Interfaces.IFreightMaster;
using LogisticsManagement.Application.FreightMaster.Dto;

namespace LogisticsManagement.Infrastructure.Repositories.FreightMaster
{
    public class FreightMasterQueryRepository : IFreightMasterQueryRepository
    {
        private readonly IDbConnection _dbConnection;

        public FreightMasterQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<(List<FreightMasterDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var whereClause = "fm.IsDeleted = 0";
            if (!string.IsNullOrWhiteSpace(searchTerm))
                whereClause += " AND (mmMode.Description LIKE @Search OR mmMethod.Description LIKE @Search)";

            var query = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Logistics.FreightMaster fm
                LEFT JOIN Logistics.MiscMaster mmMode ON fm.FreightModeId = mmMode.Id AND mmMode.IsDeleted = 0
                LEFT JOIN Logistics.MiscMaster mmMethod ON fm.RateMethodId = mmMethod.Id AND mmMethod.IsDeleted = 0
                WHERE {whereClause};

                SELECT fm.Id, fm.FreightModeId, mmMode.Description AS FreightModeName,
                    fm.RateMethodId, mmMethod.Description AS RateMethodName,
                    fm.Rate, fm.ModuleId,
                    fm.IsActive, fm.IsDeleted,
                    fm.CreatedBy, fm.CreatedDate, fm.CreatedByName,
                    fm.ModifiedBy, fm.ModifiedDate, fm.ModifiedByName
                FROM Logistics.FreightMaster fm
                LEFT JOIN Logistics.MiscMaster mmMode ON fm.FreightModeId = mmMode.Id AND mmMode.IsDeleted = 0
                LEFT JOIN Logistics.MiscMaster mmMethod ON fm.RateMethodId = mmMethod.Id AND mmMethod.IsDeleted = 0
                WHERE {whereClause}
                ORDER BY fm.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;
            ";

            var parameters = new { Search = $"%{searchTerm}%", Offset = (pageNumber - 1) * pageSize, PageSize = pageSize };
            var result = await _dbConnection.QueryMultipleAsync(query, parameters);
            var list = (await result.ReadAsync<FreightMasterDto>()).ToList();
            var totalCount = await result.ReadFirstAsync<int>();

            return (list, totalCount);
        }

        public async Task<FreightMasterDto?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT fm.Id, fm.FreightModeId, mmMode.Description AS FreightModeName,
                    fm.RateMethodId, mmMethod.Description AS RateMethodName,
                    fm.Rate, fm.ModuleId,
                    fm.IsActive, fm.IsDeleted,
                    fm.CreatedBy, fm.CreatedDate, fm.CreatedByName,
                    fm.ModifiedBy, fm.ModifiedDate, fm.ModifiedByName
                FROM Logistics.FreightMaster fm
                LEFT JOIN Logistics.MiscMaster mmMode ON fm.FreightModeId = mmMode.Id AND mmMode.IsDeleted = 0
                LEFT JOIN Logistics.MiscMaster mmMethod ON fm.RateMethodId = mmMethod.Id AND mmMethod.IsDeleted = 0
                WHERE fm.Id = @Id AND fm.IsDeleted = 0";

            return await _dbConnection.QueryFirstOrDefaultAsync<FreightMasterDto>(sql, new { Id = id });
        }

        public async Task<IReadOnlyList<FreightMasterLookupDto>> AutocompleteAsync(string term, int? moduleId, CancellationToken ct)
        {
            var whereClause = "fm.IsDeleted = 0 AND fm.IsActive = 1";
            if (!string.IsNullOrWhiteSpace(term))
                whereClause += " AND (mmMode.Description LIKE @Term OR mmMethod.Description LIKE @Term)";
            if (moduleId.HasValue && moduleId.Value > 0)
                whereClause += " AND fm.ModuleId = @ModuleId";

            var sql = $@"
                SELECT fm.Id, mmMode.Description AS FreightModeName,
                    mmMethod.Description AS RateMethodName, fm.Rate
                FROM Logistics.FreightMaster fm
                LEFT JOIN Logistics.MiscMaster mmMode ON fm.FreightModeId = mmMode.Id AND mmMode.IsDeleted = 0
                LEFT JOIN Logistics.MiscMaster mmMethod ON fm.RateMethodId = mmMethod.Id AND mmMethod.IsDeleted = 0
                WHERE {whereClause}
                ORDER BY fm.Id ASC";

            var result = await _dbConnection.QueryAsync<FreightMasterLookupDto>(
                new CommandDefinition(sql, new { Term = $"%{term}%", ModuleId = moduleId }, cancellationToken: ct));
            return result.ToList();
        }

        public async Task<bool> CompositeKeyExistsAsync(int freightModeId, int rateMethodId, int moduleId, int? id = null)
        {
            var sql = @"
                SELECT COUNT(1)
                FROM Logistics.FreightMaster
                WHERE FreightModeId = @FreightModeId AND RateMethodId = @RateMethodId
                AND ModuleId = @ModuleId AND IsDeleted = 0";

            if (id.HasValue && id.Value > 0)
                sql += " AND Id != @Id";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { FreightModeId = freightModeId, RateMethodId = rateMethodId, ModuleId = moduleId, Id = id });
            return count > 0;
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Logistics.FreightMaster
                WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<bool> MiscMasterExistsAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Logistics.MiscMaster
                WHERE Id = @Id AND IsDeleted = 0 AND IsActive = 1";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count > 0;
        }

        public async Task<bool> IsValidModeMethodCombinationAsync(int freightModeId, int rateMethodId)
        {
            const string sql = @"
                SELECT Description
                FROM Logistics.MiscMaster
                WHERE Id = @Id AND IsDeleted = 0 AND IsActive = 1";

            var modeDescription = await _dbConnection.QueryFirstOrDefaultAsync<string>(sql, new { Id = freightModeId });
            var methodDescription = await _dbConnection.QueryFirstOrDefaultAsync<string>(sql, new { Id = rateMethodId });

            if (string.IsNullOrEmpty(modeDescription) || string.IsNullOrEmpty(methodDescription))
                return false;

            var mode = modeDescription.Trim().ToUpperInvariant();
            var method = methodDescription.Trim().ToUpperInvariant();

            // PER_KM mode → only PER_KM method allowed
            if (mode == "PER_KM")
                return method == "PER_KM";

            // INNER mode → PER_KG, PER_BAG, FIXED allowed
            if (mode == "INNER")
                return method is "PER_KG" or "PER_BAG" or "FIXED";

            // OUTER mode → PER_KG, PER_BAG, FIXED allowed
            if (mode == "OUTER")
                return method is "PER_KG" or "PER_BAG" or "FIXED";

            return false;
        }
    }
}
