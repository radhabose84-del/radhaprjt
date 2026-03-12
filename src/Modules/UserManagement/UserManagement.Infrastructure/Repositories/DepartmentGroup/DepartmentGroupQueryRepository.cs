#nullable disable
using System.Data;
using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Application.Common.Interfaces.IDepartmentGroup;
using Dapper;

namespace UserManagement.Infrastructure.Repositories.DepartmentGroup
{
    public class DepartmentGroupQueryRepository : IDepartmentGroupQueryRepository
    {

        private readonly IDbConnection _dbConnection;
        private readonly IIPAddressService _ipAddressService;



        public DepartmentGroupQueryRepository(IDbConnection dbConnection, IIPAddressService ipAddressService)
        {

            _dbConnection = dbConnection;
            _ipAddressService = ipAddressService;

        }


        public async Task<UserManagement.Domain.Entities.DepartmentGroup> GetDepartmentGroupByIdAsync(int id)
        {
            var sql = @"
                SELECT 
                    Id,
                    DepartmentGroupCode,
                    DepartmentGroupName,
                    IsActive,
                    CreatedBy,
                    CreatedDate AS CreatedAt,
                    CreatedByName,
                    CreatedIP,
                    ModifiedBy,
                    ModifiedDate AS ModifiedAt,
                    ModifiedByName,
                    ModifiedIP,
                    IsDeleted
                FROM AppData.DepartmentGroup
                WHERE Id = @Id AND IsDeleted = 0";

            return await _dbConnection.QueryFirstOrDefaultAsync<UserManagement.Domain.Entities.DepartmentGroup>(sql, new { Id = id });
        }

        public async Task<(List<UserManagement.Domain.Entities.DepartmentGroup>, int)> GetAllDepartmentGroupAsync(int pageNumber, int pageSize, string searchTerm)
        {
            var query = $$"""
                    DECLARE @TotalCount INT;

                    SELECT @TotalCount = COUNT(*) 
                    FROM AppData.DepartmentGroup
                    WHERE IsDeleted = 0
                    {{(string.IsNullOrEmpty(searchTerm) ? "" : "AND (DepartmentGroupCode LIKE @Search OR DepartmentGroupName LIKE @Search)")}};

                    SELECT 
                        [Id],
                        [DepartmentGroupCode],
                        [DepartmentGroupName],
                        [IsActive],
                        [CreatedBy],
                        [CreatedDate] AS CreatedAt,
                        [CreatedByName],
                        [CreatedIP],
                        [ModifiedBy],
                        [ModifiedDate] AS ModifiedAt,
                        [ModifiedByName],
                        [ModifiedIP],
                        [IsDeleted]
                    FROM AppData.DepartmentGroup
                    WHERE IsDeleted = 0
                    {{(string.IsNullOrEmpty(searchTerm) ? "" : "AND (DepartmentGroupCode LIKE @Search OR DepartmentGroupName LIKE @Search)")}}
                    ORDER BY Id DESC
                    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                    SELECT @TotalCount AS TotalCount;
                    """;

            var parameters = new
            {
                Search = $"%{searchTerm}%",
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            var result = await _dbConnection.QueryMultipleAsync(query, parameters);

            var departmentGroupList = (await result.ReadAsync<UserManagement.Domain.Entities.DepartmentGroup>()).ToList();
            var totalCount = await result.ReadFirstAsync<int>();

            return (departmentGroupList, totalCount);
        }



        public async Task<bool> SoftDeleteValidation(int Id)
        {
            const string query = @"
                           SELECT 1 
                           FROM [AppData].[DepartmentGroup] 
                         WHERE Id = @Id AND   IsDeleted = 0  ;";

            using var multi = await _dbConnection.QueryMultipleAsync(query, new { Id = Id });

            var DepartmentGroupExists = await multi.ReadFirstOrDefaultAsync<int?>();

            return DepartmentGroupExists.HasValue;
        }

        public async Task<List<UserManagement.Domain.Entities.DepartmentGroup>> GetAllDepartmentGroupAsync(string searchPattern)
        {
            const string query = @"
                    SELECT 
                        [Id],
                        [DepartmentGroupCode],
                        [DepartmentGroupName],
                        [IsActive],
                        [CreatedBy],
                        [CreatedDate]       AS CreatedAt,
                        [CreatedByName],
                        [CreatedIP],
                        [ModifiedBy],
                        [ModifiedDate]      AS ModifiedAt,
                        [ModifiedByName],
                        [ModifiedIP],
                        [IsDeleted]
                    FROM AppData.DepartmentGroup
                    WHERE 
                        (DepartmentGroupCode LIKE @SearchPattern OR DepartmentGroupName LIKE @SearchPattern)
                        AND IsDeleted = 0 AND IsActive = 1
                    ORDER BY Id DESC";

            var result = await _dbConnection.QueryAsync<UserManagement.Domain.Entities.DepartmentGroup>(
                query,
                new { SearchPattern = $"%{searchPattern}%" }
            );

            return result.ToList();
        }

        
        public async Task<UserManagement.Domain.Entities.DepartmentGroup> GetByDepartmentGroupNameAsync(string departmentGroupName)
    {
        const string query = @"
            SELECT TOP 1 * 
            FROM AppData.DepartmentGroup 
            WHERE DepartmentGroupName = @DepartmentGroupName AND IsDeleted = 0";

        var result = await _dbConnection.QueryFirstOrDefaultAsync<UserManagement.Domain.Entities.DepartmentGroup>(
            query,
            new { DepartmentGroupName = departmentGroupName }
        );

        return result;
    }

        public async Task<bool> IsLinkedWithDepartmentsAsync(int departmentGroupId)
        {
            const string query = @"
        SELECT TOP 1 1
        FROM AppData.Department
        WHERE IsDeleted = 0 AND DepartmentGroupId = @departmentGroupId;
        ";

            var result = await _dbConnection.QueryFirstOrDefaultAsync<int?>(query, new { departmentGroupId });
            return result.HasValue;
        }

                  

       
    }
}