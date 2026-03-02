using System.Data;
using Dapper;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Application.MiscMaster.Dto;

namespace SalesManagement.Infrastructure.Repositories.MiscMaster
{
    public class MiscMasterQueryRepository : IMiscMasterQueryRepository
    {
        private readonly IDbConnection _dbConnection;

        public MiscMasterQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<(List<MiscMasterDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm, int? miscTypeId = null)
        {
            var whereClause = "mm.IsDeleted = 0";
            if (!string.IsNullOrWhiteSpace(searchTerm))
                whereClause += " AND (mm.Code LIKE @Search OR mm.Description LIKE @Search)";
            if (miscTypeId.HasValue && miscTypeId.Value > 0)
                whereClause += " AND mm.MiscTypeId = @MiscTypeId";

            var query = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Sales.MiscMaster mm
                WHERE {whereClause};

                SELECT mm.Id, mm.MiscTypeId, mm.Code, mm.Description, mm.SortOrder,
                    mtm.MiscTypeCode, mtm.Description AS MiscTypeDescription,
                    mm.IsActive, mm.IsDeleted,
                    mm.CreatedBy, mm.CreatedDate, mm.CreatedByName, mm.CreatedIP,
                    mm.ModifiedBy, mm.ModifiedDate, mm.ModifiedByName, mm.ModifiedIP
                FROM Sales.MiscMaster mm
                INNER JOIN Sales.MiscTypeMaster mtm ON mm.MiscTypeId = mtm.Id AND mtm.IsDeleted = 0
                WHERE {whereClause}
                ORDER BY mm.MiscTypeId ASC, mm.SortOrder ASC, mm.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;
            ";

            var parameters = new { Search = $"%{searchTerm}%", MiscTypeId = miscTypeId, Offset = (pageNumber - 1) * pageSize, PageSize = pageSize };
            var result = await _dbConnection.QueryMultipleAsync(query, parameters);
            var list = (await result.ReadAsync<MiscMasterDto>()).ToList();
            var totalCount = await result.ReadFirstAsync<int>();

            return (list, totalCount);
        }

        public async Task<MiscMasterDto?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT mm.Id, mm.MiscTypeId, mm.Code, mm.Description, mm.SortOrder,
                    mtm.MiscTypeCode, mtm.Description AS MiscTypeDescription,
                    mm.IsActive, mm.IsDeleted,
                    mm.CreatedBy, mm.CreatedDate, mm.CreatedByName, mm.CreatedIP,
                    mm.ModifiedBy, mm.ModifiedDate, mm.ModifiedByName, mm.ModifiedIP
                FROM Sales.MiscMaster mm
                INNER JOIN Sales.MiscTypeMaster mtm ON mm.MiscTypeId = mtm.Id AND mtm.IsDeleted = 0
                WHERE mm.Id = @Id AND mm.IsDeleted = 0";

            return await _dbConnection.QueryFirstOrDefaultAsync<MiscMasterDto>(sql, new { Id = id });
        }

        public async Task<IReadOnlyList<MiscMasterLookupDto>> AutocompleteAsync(string term, string? miscTypeCode, CancellationToken ct)
        {
            var whereClause = "mm.IsDeleted = 0 AND mm.IsActive = 1";
            if (!string.IsNullOrWhiteSpace(term))
                whereClause += " AND (mm.Code LIKE @Term OR mm.Description LIKE @Term)";
            if (!string.IsNullOrWhiteSpace(miscTypeCode))
                whereClause += " AND mtm.MiscTypeCode = @MiscTypeCode";                

            var sql = $@"
                SELECT TOP 20 mm.Id, mm.MiscTypeId, mtm.MiscTypeCode, mm.Code, mm.Description
                FROM Sales.MiscMaster mm
                INNER JOIN Sales.MiscTypeMaster mtm ON mm.MiscTypeId = mtm.Id AND mtm.IsDeleted = 0
                WHERE {whereClause}
                ORDER BY mm.MiscTypeId ASC, mm.SortOrder ASC";

            var result = await _dbConnection.QueryAsync<MiscMasterLookupDto>(
                new CommandDefinition(sql, new { Term = $"%{term}%", MiscTypeCode = miscTypeCode }, cancellationToken: ct));
            return result.ToList();
        }

        public async Task<bool> AlreadyExistsAsync(string code, int miscTypeId, int? id = null)
        {
            var sql = @"
                SELECT COUNT(1)
                FROM Sales.MiscMaster
                WHERE Code = @Code AND MiscTypeId = @MiscTypeId
                AND IsDeleted = 0";

            if (id.HasValue && id.Value > 0)
                sql += " AND Id != @Id";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Code = code.Trim(), MiscTypeId = miscTypeId, Id = id });
            return count > 0;
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Sales.MiscMaster
                WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<bool> MiscTypeExistsAsync(int miscTypeId)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Sales.MiscTypeMaster
                WHERE Id = @MiscTypeId AND IsDeleted = 0 AND IsActive = 1";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { MiscTypeId = miscTypeId });
            return count > 0;
        }

        public async Task<bool> SoftDeleteValidationAsync(int id)
        {
            // Returns true if MiscMaster is linked to active dependent records (blocking deletion).
            // Currently MiscMaster has no FK children — always returns false (safe to delete).
            await Task.CompletedTask;
            return false;
        }

        public async Task<SalesManagement.Domain.Entities.MiscMaster?> GetMiscMasterByName(string miscTypeCode, string miscTypeName)
        {
            const string sql = @"
                SELECT M.Id, M.Code, M.Description
                FROM Sales.MiscMaster AS M
                INNER JOIN Sales.MiscTypeMaster AS MT ON MT.Id = M.MiscTypeId
                WHERE M.IsDeleted = 0 AND M.IsActive = 1
                AND MT.IsDeleted = 0
                AND LOWER(MT.MiscTypeCode) = LOWER(@MiscTypeCode)
                AND LOWER(M.Code) = LOWER(@MiscTypeName);";

            return await _dbConnection.QueryFirstOrDefaultAsync<SalesManagement.Domain.Entities.MiscMaster>(sql, new { MiscTypeCode = miscTypeCode, MiscTypeName = miscTypeName });
        }
    }
}
