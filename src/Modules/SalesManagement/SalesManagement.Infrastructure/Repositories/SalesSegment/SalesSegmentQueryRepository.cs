#nullable disable

using Dapper;
using SalesManagement.Application.Common.Interfaces.ISalesSegment;
using SalesManagement.Application.SalesSegment.Dto;
using System.Data;

namespace SalesManagement.Infrastructure.Repositories.SalesSegment
{
    public class SalesSegmentQueryRepository : ISalesSegmentQueryRepository
    {
        private readonly IDbConnection _dbConnection;

        public SalesSegmentQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<(List<SalesSegmentDto>, int)> GetAllAsync(int pageNumber, int pageSize, string searchTerm)
        {
            const string countSql = @"
                SELECT COUNT(1)
                FROM Sales.SalesSegment ss
                WHERE ss.IsDeleted = 0
                AND (@SearchTerm IS NULL OR
                     ss.SegmentName LIKE '%' + @SearchTerm + '%' OR
                     EXISTS (SELECT 1 FROM Sales.SalesOrganisation so WHERE so.Id = ss.SalesOrganisationId AND so.SalesOrganisationName LIKE '%' + @SearchTerm + '%') OR
                     EXISTS (SELECT 1 FROM Sales.SalesChannel sc WHERE sc.Id = ss.SalesChannelId AND sc.SalesChannelName LIKE '%' + @SearchTerm + '%') OR
                     EXISTS (SELECT 1 FROM Sales.BusinessUnit bu WHERE bu.Id = ss.BusinessUnitId AND bu.BusinessUnitName LIKE '%' + @SearchTerm + '%'))";

            const string dataSql = @"
                SELECT
                    ss.Id,
                    ss.SalesOrganisationId,
                    so.SalesOrganisationName,
                    ss.SalesChannelId,
                    sc.SalesChannelName,
                    ss.BusinessUnitId,
                    bu.BusinessUnitName,
                    ss.CurrencyId,
                    ss.ValidFrom,
                    ss.SegmentName,
                    CAST(ss.IsActive AS BIT) AS IsActive,
                    CAST(ss.IsDeleted AS BIT) AS IsDeleted,
                    ss.CreatedBy,
                    ss.CreatedDate,
                    ss.CreatedByName,
                    ss.ModifiedBy,
                    ss.ModifiedDate,
                    ss.ModifiedByName
                FROM Sales.SalesSegment ss
                INNER JOIN Sales.SalesOrganisation so ON ss.SalesOrganisationId = so.Id
                INNER JOIN Sales.SalesChannel sc ON ss.SalesChannelId = sc.Id
                INNER JOIN Sales.BusinessUnit bu ON ss.BusinessUnitId = bu.Id
                WHERE ss.IsDeleted = 0
                AND (@SearchTerm IS NULL OR
                     ss.SegmentName LIKE '%' + @SearchTerm + '%' OR
                     so.SalesOrganisationName LIKE '%' + @SearchTerm + '%' OR
                     sc.SalesChannelName LIKE '%' + @SearchTerm + '%' OR
                     bu.BusinessUnitName LIKE '%' + @SearchTerm + '%')
                ORDER BY ss.CreatedDate DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var parameters = new
            {
                SearchTerm = string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm,
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            var totalCount = await _dbConnection.ExecuteScalarAsync<int>(countSql, parameters);
            var data = (await _dbConnection.QueryAsync<SalesSegmentDto>(dataSql, parameters)).ToList();

            return (data, totalCount);
        }

        public async Task<SalesSegmentDto> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT
                    ss.Id,
                    ss.SalesOrganisationId,
                    so.SalesOrganisationName,
                    ss.SalesChannelId,
                    sc.SalesChannelName,
                    ss.BusinessUnitId,
                    bu.BusinessUnitName,
                    ss.CurrencyId,
                    ss.ValidFrom,
                    ss.SegmentName,
                    CAST(ss.IsActive AS BIT) AS IsActive,
                    CAST(ss.IsDeleted AS BIT) AS IsDeleted,
                    ss.CreatedBy,
                    ss.CreatedDate,
                    ss.CreatedByName,
                    ss.ModifiedBy,
                    ss.ModifiedDate,
                    ss.ModifiedByName
                FROM Sales.SalesSegment ss
                INNER JOIN Sales.SalesOrganisation so ON ss.SalesOrganisationId = so.Id
                INNER JOIN Sales.SalesChannel sc ON ss.SalesChannelId = sc.Id
                INNER JOIN Sales.BusinessUnit bu ON ss.BusinessUnitId = bu.Id
                WHERE ss.Id = @Id AND ss.IsDeleted = 0";

            return await _dbConnection.QueryFirstOrDefaultAsync<SalesSegmentDto>(sql, new { Id = id });
        }

        public async Task<IReadOnlyList<SalesSegmentLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            const string sql = @"
                SELECT TOP 20
                    Id,
                    SegmentName,
                    SalesOrganisationId,
                    SalesChannelId,
                    BusinessUnitId
                FROM Sales.SalesSegment
                WHERE IsActive = 1
                AND IsDeleted = 0
                AND (@Term IS NULL OR SegmentName LIKE '%' + @Term + '%')
                ORDER BY SegmentName";

            var result = await _dbConnection.QueryAsync<SalesSegmentLookupDto>(
                sql,
                new { Term = string.IsNullOrWhiteSpace(term) ? null : term });

            return result.ToList();
        }

        public async Task<bool> CompositeKeyExistsAsync(int salesOrgId, int channelId, int buId, int? excludeId = null)
        {
            const string sql = @"
                SELECT CAST(CASE WHEN EXISTS (
                    SELECT 1
                    FROM Sales.SalesSegment
                    WHERE SalesOrganisationId = @SalesOrgId
                    AND SalesChannelId = @ChannelId
                    AND BusinessUnitId = @BuId
                    AND IsDeleted = 0
                    AND (@ExcludeId IS NULL OR Id != @ExcludeId)
                ) THEN 1 ELSE 0 END AS BIT)";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new
            {
                SalesOrgId = salesOrgId,
                ChannelId = channelId,
                BuId = buId,
                ExcludeId = excludeId
            });
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT CAST(CASE WHEN EXISTS (
                    SELECT 1
                    FROM Sales.SalesSegment
                    WHERE Id = @Id AND IsDeleted = 0
                ) THEN 0 ELSE 1 END AS BIT)";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = id });
        }

        public async Task<bool> SalesOrganisationExistsAsync(int id)
        {
            const string sql = @"
                SELECT CAST(CASE WHEN EXISTS (
                    SELECT 1
                    FROM Sales.SalesOrganisation
                    WHERE Id = @Id AND IsDeleted = 0
                ) THEN 1 ELSE 0 END AS BIT)";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = id });
        }

        public async Task<bool> SalesChannelExistsAsync(int id)
        {
            const string sql = @"
                SELECT CAST(CASE WHEN EXISTS (
                    SELECT 1
                    FROM Sales.SalesChannel
                    WHERE Id = @Id AND IsDeleted = 0
                ) THEN 1 ELSE 0 END AS BIT)";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = id });
        }

        public async Task<bool> BusinessUnitExistsAsync(int id)
        {
            const string sql = @"
                SELECT CAST(CASE WHEN EXISTS (
                    SELECT 1
                    FROM Sales.BusinessUnit
                    WHERE Id = @Id AND IsDeleted = 0
                ) THEN 1 ELSE 0 END AS BIT)";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = id });
        }
    }
}
