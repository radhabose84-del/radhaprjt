using System.Data;
using Dapper;
using Contracts.Interfaces.Lookups.Users;
using SalesManagement.Application.Common.Interfaces.ISalesOrganisation;
using SalesManagement.Application.SalesOrganisation.Dto;

namespace SalesManagement.Infrastructure.Repositories.SalesOrganisation
{
    public class SalesOrganisationQueryRepository : ISalesOrganisationQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly ICompanyLookup _companyLookup;

        public SalesOrganisationQueryRepository(IDbConnection dbConnection, ICompanyLookup companyLookup)
        {
            _dbConnection = dbConnection;
            _companyLookup = companyLookup;
        }

        public async Task<(List<SalesOrganisationDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var companies = await _companyLookup.GetAllCompanyAsync();
            var companyDict = companies.ToDictionary(c => c.CompanyId, c => c.CompanyName);

            var query = $$"""
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Sales.SalesOrganisation so
                WHERE so.IsDeleted = 0
                {{(string.IsNullOrWhiteSpace(searchTerm) ? "" : "AND (so.SalesOrganisationCode LIKE @Search OR so.SalesOrganisationName LIKE @Search)")}};

                SELECT so.Id, so.SalesOrganisationCode, so.SalesOrganisationName,
                    so.CompanyId, so.Description,
                    so.IsActive, so.IsDeleted,
                    so.CreatedBy, so.CreatedDate, so.CreatedByName, so.CreatedIP,
                    so.ModifiedBy, so.ModifiedDate, so.ModifiedByName, so.ModifiedIP
                FROM Sales.SalesOrganisation so
                WHERE so.IsDeleted = 0
                {{(string.IsNullOrWhiteSpace(searchTerm) ? "" : "AND (so.SalesOrganisationCode LIKE @Search OR so.SalesOrganisationName LIKE @Search)")}}
                ORDER BY so.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;
            """;

            var parameters = new { Search = $"%{searchTerm}%", Offset = (pageNumber - 1) * pageSize, PageSize = pageSize };
            var result = await _dbConnection.QueryMultipleAsync(query, parameters);
            var list = (await result.ReadAsync<SalesOrganisationDto>()).ToList();
            var totalCount = await result.ReadFirstAsync<int>();

            foreach (var item in list)
            {
                item.CompanyName = companyDict.TryGetValue(item.CompanyId, out var name) ? name : null;
            }

            return (list, totalCount);
        }

        public async Task<SalesOrganisationDto?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT so.Id, so.SalesOrganisationCode, so.SalesOrganisationName,
                    so.CompanyId, so.Description,
                    so.IsActive, so.IsDeleted,
                    so.CreatedBy, so.CreatedDate, so.CreatedByName, so.CreatedIP,
                    so.ModifiedBy, so.ModifiedDate, so.ModifiedByName, so.ModifiedIP
                FROM Sales.SalesOrganisation so
                WHERE so.Id = @Id AND so.IsDeleted = 0";

            var dto = await _dbConnection.QueryFirstOrDefaultAsync<SalesOrganisationDto>(sql, new { Id = id });

            if (dto != null)
            {
                var companies = await _companyLookup.GetAllCompanyAsync();
                var company = companies.FirstOrDefault(c => c.CompanyId == dto.CompanyId);
                dto.CompanyName = company?.CompanyName;
            }

            return dto;
        }

        public async Task<bool> AlreadyExistsAsync(string salesOrganisationCode, int? id = null)
        {
            var sql = @"
                SELECT COUNT(1)
                FROM Sales.SalesOrganisation
                WHERE SalesOrganisationCode = @Code
                AND IsDeleted = 0";

            if (id.HasValue && id.Value > 0)
                sql += " AND Id != @Id";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Code = salesOrganisationCode.Trim(), Id = id });
            return count > 0;
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Sales.SalesOrganisation
                WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<bool> CompanyExistsAsync(int companyId)
        {
            var companies = await _companyLookup.GetAllCompanyAsync();
            return companies.Any(c => c.CompanyId == companyId);
        }

        public async Task<IReadOnlyList<SalesOrganisationLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            const string sql = @"
                SELECT TOP 20 Id, SalesOrganisationCode, SalesOrganisationName
                FROM Sales.SalesOrganisation
                WHERE IsDeleted = 0 AND IsActive = 1
                AND (SalesOrganisationCode LIKE @Term OR SalesOrganisationName LIKE @Term)
                ORDER BY SalesOrganisationName ASC";

            var result = await _dbConnection.QueryAsync<SalesOrganisationLookupDto>(
                new CommandDefinition(sql, new { Term = $"%{term}%" }, cancellationToken: ct));
            return result.ToList();
        }

        public async Task<bool> SoftDeleteValidationAsync(int id)
        {
            // Returns true if SalesOrganisation is linked to active dependent records (blocking deletion).
            // Currently SalesOrganisation has no FK children — always returns false (safe to delete).
            await Task.CompletedTask;
            return false;
        }
    }
}
