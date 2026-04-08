using System.Data;
using Contracts.Dtos.Lookups.Production;
using Contracts.Interfaces.Validations.SalesManagement;
using Dapper;
using ProductionManagement.Application.Common.Interfaces.IPackType;
using ProductionManagement.Application.PackType.Dto;

namespace ProductionManagement.Infrastructure.Repositories.PackType
{
    public class PackTypeQueryRepository : IPackTypeQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IPackTypeSalesValidation _salesValidation;

        public PackTypeQueryRepository(IDbConnection dbConnection, IPackTypeSalesValidation salesValidation)
        {
            _dbConnection = dbConnection;
            _salesValidation = salesValidation;
        }

        public async Task<(List<PackTypeDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var query = $$"""
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Production.PackType pt
                WHERE pt.IsDeleted = 0
                {{(string.IsNullOrWhiteSpace(searchTerm) ? "" : "AND (pt.PackTypeCode LIKE @Search OR pt.PackTypeName LIKE @Search)")}};

                SELECT pt.Id, pt.PackTypeCode, pt.PackTypeName,
                    pt.NetWeight, pt.TareWeight, pt.GrossWeight,
                    pt.ConesPerBag, pt.PackMaterialId,
                    mm.Description AS PackMaterialName,
                    pt.ProductionAllowed,
                    pt.IsActive, pt.IsDeleted,
                    pt.CreatedBy, pt.CreatedDate, pt.CreatedByName, pt.CreatedIP,
                    pt.ModifiedBy, pt.ModifiedDate, pt.ModifiedByName, pt.ModifiedIP
                FROM Production.PackType pt
                LEFT JOIN Production.MiscMaster mm ON pt.PackMaterialId = mm.Id AND mm.IsDeleted = 0
                WHERE pt.IsDeleted = 0
                {{(string.IsNullOrWhiteSpace(searchTerm) ? "" : "AND (pt.PackTypeCode LIKE @Search OR pt.PackTypeName LIKE @Search)")}}
                ORDER BY pt.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;
            """;

            var parameters = new { Search = $"%{searchTerm}%", Offset = (pageNumber - 1) * pageSize, PageSize = pageSize };
            var result = await _dbConnection.QueryMultipleAsync(query, parameters);
            var list = (await result.ReadAsync<PackTypeDto>()).ToList();
            var totalCount = await result.ReadFirstAsync<int>();

            return (list, totalCount);
        }

        public async Task<PackTypeDto?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT pt.Id, pt.PackTypeCode, pt.PackTypeName,
                    pt.NetWeight, pt.TareWeight, pt.GrossWeight,
                    pt.ConesPerBag, pt.PackMaterialId,
                    mm.Description AS PackMaterialName,
                    pt.ProductionAllowed,
                    pt.IsActive, pt.IsDeleted,
                    pt.CreatedBy, pt.CreatedDate, pt.CreatedByName, pt.CreatedIP,
                    pt.ModifiedBy, pt.ModifiedDate, pt.ModifiedByName, pt.ModifiedIP
                FROM Production.PackType pt
                LEFT JOIN Production.MiscMaster mm ON pt.PackMaterialId = mm.Id AND mm.IsDeleted = 0
                WHERE pt.Id = @Id AND pt.IsDeleted = 0";

            return await _dbConnection.QueryFirstOrDefaultAsync<PackTypeDto>(sql, new { Id = id });
        }

        public async Task<IReadOnlyList<PackTypeLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            const string sql = @"
                SELECT Id, PackTypeCode, PackTypeName,NetWeight,TareWeight,GrossWeight,ConesPerBag
                FROM Production.PackType
                WHERE IsDeleted = 0 AND IsActive = 1
                AND (PackTypeCode LIKE @Term OR PackTypeName LIKE @Term)
                ORDER BY PackTypeName ASC";

            var result = await _dbConnection.QueryAsync<PackTypeLookupDto>(
                new CommandDefinition(sql, new { Term = $"%{term}%" }, cancellationToken: ct));
            return result.ToList();
        }

        public async Task<bool> AlreadyExistsAsync(string packTypeCode, int? id = null)
        {
            var sql = @"
                SELECT COUNT(1)
                FROM Production.PackType
                WHERE PackTypeCode = @Code
                AND IsDeleted = 0";

            if (id.HasValue && id.Value > 0)
                sql += " AND Id != @Id";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Code = packTypeCode.Trim(), Id = id });
            return count > 0;
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Production.PackType
                WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<bool> PackMaterialExistsAsync(int packMaterialId)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Production.MiscMaster
                WHERE Id = @Id AND IsActive = 1 AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = packMaterialId });
            return count > 0;
        }

        public async Task<bool> SoftDeleteValidationAsync(int id)
        {
            const string sql = @"
                SELECT CASE WHEN
                    EXISTS (SELECT 1 FROM [Production].[ProductionPackDetail] WHERE PackTypeId = @Id AND IsDeleted = 0)
                    OR EXISTS (SELECT 1 FROM [Production].[RepackingHeader] WHERE (PackTypeId = @Id OR OldPackTypeId = @Id) AND IsDeleted = 0)
                THEN 1 ELSE 0 END";

            var sameModule = await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = id });
            if (sameModule) return true;

            return await _salesValidation.HasLinkedPackTypeAsync(id);
        }

        public async Task<bool> IsPackTypeLinkedAsync(int id)
        {
            const string sql = @"
                SELECT CASE WHEN
                    EXISTS (SELECT 1 FROM [Production].[ProductionPackDetail] WHERE PackTypeId = @Id AND IsDeleted = 0 AND IsActive = 1)
                    OR EXISTS (SELECT 1 FROM [Production].[RepackingHeader] WHERE (PackTypeId = @Id OR OldPackTypeId = @Id) AND IsDeleted = 0 AND IsActive = 1)
                THEN 1 ELSE 0 END";

            var sameModule = await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = id });
            if (sameModule) return true;

            return await _salesValidation.HasActivePackTypeAsync(id);
        }
    }
}
