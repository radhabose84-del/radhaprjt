using System.Data;
using Dapper;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces;

namespace UserManagement.Infrastructure.Repositories.Lookups.Users
{
    internal class DepartmentLookupRepository : IDepartmentLookup
    {
        private readonly IDbConnection _dbConnection;
        private readonly IIPAddressService _ipAddressService;

        public DepartmentLookupRepository(IDbConnection dbConnection, IIPAddressService ipAddressService)
        {
            _dbConnection = dbConnection;
            _ipAddressService = ipAddressService;
        }

        public async Task<List<DepartmentLookupDto>> GetAllDepartmentAsync()
        {
            var CompanyId = _ipAddressService.GetCompanyId() ?? 0;
            const string sql = @"
                SELECT
                    Id        AS DepartmentId,
                    DeptName  AS DepartmentName,
                    ShortName AS ShortName,DepartmentGroupId
                FROM [AppData].[Department]
                WHERE IsDeleted = 0 AND CompanyId=@CompanyId 
                ORDER BY DeptName ASC;
            ";

             var parameters = new
                {        
                    CompanyId = CompanyId
                };

            var result = await _dbConnection.QueryAsync<DepartmentLookupDto>(sql, parameters);
            return result.ToList();           
        }

        public async Task<DepartmentLookupDto?> GetByIdAsync(int departmentId, CancellationToken ct = default)
        {
            const string sql = @"
                SELECT TOP 1
                    Id       AS DepartmentId,
                    DeptName AS DepartmentName,ShortName,Departmentgroupid
                FROM [AppData].[Department]
                WHERE IsDeleted = 0 AND Id = @Id;
            ";

            return await _dbConnection.QueryFirstOrDefaultAsync<DepartmentLookupDto>(
                new CommandDefinition(sql, new { Id = departmentId }, cancellationToken: ct));
        }

        public async Task<IReadOnlyList<DepartmentLookupDto>> GetByIdsAsync(IEnumerable<int> departmentIds, CancellationToken ct = default)
        {
            var ids = departmentIds?.Distinct().ToArray() ?? Array.Empty<int>();
            if (ids.Length == 0)
                return Array.Empty<DepartmentLookupDto>();

            const string sql = @"
                SELECT
                    Id       AS DepartmentId,
                    DeptName AS  DepartmentName,ShortName,Departmentgroupid
                FROM [AppData].[Department]
                WHERE IsDeleted = 0 AND Id IN @Ids;
            ";

            var rows = await _dbConnection.QueryAsync<DepartmentLookupDto>(
                new CommandDefinition(sql, new { Ids = ids }, cancellationToken: ct));

            return rows.ToList();
        }
    }
}
