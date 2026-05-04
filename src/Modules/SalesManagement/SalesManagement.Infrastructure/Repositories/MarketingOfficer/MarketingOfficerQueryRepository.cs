using System.Data;
using Dapper;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Application.Common.Interfaces.IMarketingOfficer;
using SalesManagement.Application.MarketingOfficer.Dto;

namespace SalesManagement.Infrastructure.Repositories.MarketingOfficer
{
    public class MarketingOfficerQueryRepository : IMarketingOfficerQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IMarketingOfficerAccessFilter _accessFilter;

        public MarketingOfficerQueryRepository(
            IDbConnection dbConnection,
            IMarketingOfficerAccessFilter accessFilter)
        {
            _dbConnection = dbConnection;
            _accessFilter = accessFilter;
        }

        public async Task<(List<MarketingOfficerDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var query = $$"""
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Sales.MarketingOfficer mo
                WHERE mo.IsDeleted = 0
                {{(string.IsNullOrWhiteSpace(searchTerm) ? "" : "AND (mo.EmployeeNo LIKE @Search OR mo.EmployeeName LIKE @Search)")}};

                SELECT mo.Id, mo.EmployeeNo, mo.EmployeeName, mo.MobileNo, mo.Email,
                    mo.Unit, mo.Department, mo.Designation,
                    mo.SalesOfficeId, so.SalesOfficeName,
                    mo.IsActive, mo.IsDeleted,
                    mo.CreatedBy, mo.CreatedDate, mo.CreatedByName, mo.CreatedIP,
                    mo.ModifiedBy, mo.ModifiedDate, mo.ModifiedByName, mo.ModifiedIP
                FROM Sales.MarketingOfficer mo
                INNER JOIN Sales.SalesOffice so ON mo.SalesOfficeId = so.Id
                WHERE mo.IsDeleted = 0
                {{(string.IsNullOrWhiteSpace(searchTerm) ? "" : "AND (mo.EmployeeNo LIKE @Search OR mo.EmployeeName LIKE @Search)")}}
                ORDER BY mo.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;
            """;

            var parameters = new { Search = $"%{searchTerm}%", Offset = (pageNumber - 1) * pageSize, PageSize = pageSize };
            var result = await _dbConnection.QueryMultipleAsync(query, parameters);
            var list = (await result.ReadAsync<MarketingOfficerDto>()).ToList();
            var totalCount = await result.ReadFirstAsync<int>();

            return (list, totalCount);
        }

        public async Task<MarketingOfficerDto?> GetByIdAsync(int id)
        {
            const string sql = """
                SELECT mo.Id, mo.EmployeeNo, mo.EmployeeName, mo.MobileNo, mo.Email,
                    mo.Unit, mo.Department, mo.Designation,
                    mo.SalesOfficeId, so.SalesOfficeName,
                    mo.IsActive, mo.IsDeleted,
                    mo.CreatedBy, mo.CreatedDate, mo.CreatedByName, mo.CreatedIP,
                    mo.ModifiedBy, mo.ModifiedDate, mo.ModifiedByName, mo.ModifiedIP
                FROM Sales.MarketingOfficer mo
                INNER JOIN Sales.SalesOffice so ON mo.SalesOfficeId = so.Id
                WHERE mo.Id = @Id AND mo.IsDeleted = 0;

                SELECT osg.Id, osg.SalesGroupId, sg.SalesGroupName
                FROM Sales.OfficerSalesGroup osg
                INNER JOIN Sales.SalesGroup sg ON osg.SalesGroupId = sg.Id
                WHERE osg.MarketingOfficerId = @Id AND osg.IsDeleted = 0;
            """;

            using var multi = await _dbConnection.QueryMultipleAsync(sql, new { Id = id });
            var dto = await multi.ReadFirstOrDefaultAsync<MarketingOfficerDto>();

            if (dto != null)
            {
                dto.SalesGroups = (await multi.ReadAsync<OfficerSalesGroupDto>()).ToList();
            }

            return dto;
        }

        public async Task<IReadOnlyList<MarketingOfficerLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            // If marketing officer is logged in (EmpId in token), show only themselves
            if (await _accessFilter.ShouldApplyFilterAsync(ct))
            {
                var officerId = _accessFilter.GetCurrentMarketingOfficerId();
                if (officerId.HasValue)
                {
                    const string selfSql = """
                        SELECT TOP 1 Id, EmployeeNo, EmployeeName
                        FROM Sales.MarketingOfficer
                        WHERE Id = @OfficerId AND IsDeleted = 0 AND IsActive = 1
                    """;

                    var self = await _dbConnection.QueryAsync<MarketingOfficerLookupDto>(
                        new CommandDefinition(selfSql, new { OfficerId = officerId.Value }, cancellationToken: ct));
                    return self.ToList();
                }
            }

            // Admin/superuser (no EmpId or BypassDataAccess) — show all
            const string sql = """
                SELECT TOP 20 Id, EmployeeNo, EmployeeName
                FROM Sales.MarketingOfficer
                WHERE IsDeleted = 0 AND IsActive = 1
                AND (EmployeeName LIKE @Term OR EmployeeNo LIKE @Term)
                ORDER BY EmployeeName ASC
            """;

            var result = await _dbConnection.QueryAsync<MarketingOfficerLookupDto>(
                new CommandDefinition(sql, new { Term = $"%{term}%" }, cancellationToken: ct));
            return result.ToList();
        }

        public async Task<bool> AlreadyExistsAsync(string employeeNo, int? id = null)
        {
            var sql = """
                SELECT COUNT(1)
                FROM Sales.MarketingOfficer
                WHERE EmployeeNo = @EmployeeNo
                AND IsDeleted = 0
            """;

            if (id.HasValue && id.Value > 0)
                sql += " AND Id != @Id";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { EmployeeNo = employeeNo.Trim(), Id = id });
            return count > 0;
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = """
                SELECT COUNT(1)
                FROM Sales.MarketingOfficer
                WHERE Id = @Id AND IsDeleted = 0
            """;

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<bool> SalesOfficeExistsAsync(int salesOfficeId)
        {
            const string sql = """
                SELECT COUNT(1)
                FROM Sales.SalesOffice
                WHERE Id = @Id AND IsDeleted = 0 AND IsActive = 1
            """;

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = salesOfficeId });
            return count > 0;
        }

        public async Task<bool> SalesGroupExistsAsync(int salesGroupId)
        {
            const string sql = """
                SELECT COUNT(1)
                FROM Sales.SalesGroup
                WHERE Id = @Id AND IsDeleted = 0 AND IsActive = 1
            """;

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = salesGroupId });
            return count > 0;
        }

        public async Task<bool> SalesGroupsAllExistAsync(List<int> salesGroupIds)
        {
            const string sql = """
                SELECT COUNT(DISTINCT Id)
                FROM Sales.SalesGroup
                WHERE Id IN @Ids AND IsDeleted = 0 AND IsActive = 1
            """;

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Ids = salesGroupIds });
            return count == salesGroupIds.Distinct().Count();
        }

        public async Task<bool> SoftDeleteValidationAsync(int id)
        {
            // OfficerAgent does NOT extend BaseEntity and has no IsDeleted column — no IsDeleted filter
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM [Sales].[OfficerAgent] WHERE MarketingOfficerId = @id
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { id });
        }

        public async Task<bool> IsMarketingOfficerLinkedAsync(int id)
        {
            // OfficerAgent does NOT extend BaseEntity and has no IsDeleted column — filter by IsActive only
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM [Sales].[OfficerAgent] WHERE MarketingOfficerId = @id AND IsActive = 1
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { id });
        }

        public async Task<List<EmployeeLookupDto>> GetEmployeeLookupAsync(string oldUnitId, string? empNo)
        {
            var tvp = new DataTable();
            tvp.Columns.Add("DivCode", typeof(string));
            tvp.Columns.Add("EmpNo", typeof(string));
            tvp.Rows.Add(oldUnitId, string.IsNullOrWhiteSpace(empNo) ? DBNull.Value : empNo);

            var parameters = new DynamicParameters();
            parameters.Add("@EmployeeKeys", tvp.AsTableValuedParameter("dbo.EmployeeKeyType"));

            var result = await _dbConnection.QueryAsync<EmployeeLookupDto>(
                "dbo.GetEmployeeByDivision_TVP_salesofficers",
                parameters,
                commandType: CommandType.StoredProcedure,
                commandTimeout: 120);

            return result.ToList();
        }
    }
}
