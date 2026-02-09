using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ProjectManagement.Application.Common.Interfaces;
using ProjectManagement.Application.Common.Interfaces.IProjectWorkBreakdownStructure;
using ProjectManagement.Application.ProjectWorkBreakdownStructure.Queries.Dtos;
using ProjectManagement.Application.ProjectWorkBreakdownStructure.Queries.GetWbsLookup;
using Dapper;

namespace ProjectManagement.Infrastructure.Repositories.ProjectWorkBreakdownStructure
{
    public class ProjectWorkBreakdownStructureQueryRepository : IProjectWorkBreakdownStructureQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IIPAddressService _iPAddressService;

        public ProjectWorkBreakdownStructureQueryRepository(IDbConnection dbConnection, IIPAddressService iPAddressService)
        {
            _dbConnection = dbConnection;
            _iPAddressService = iPAddressService;
        }

        public async Task<(IReadOnlyList<ProjectWorkBreakdownStructureDto> Items, int TotalCount)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 20;

            var query = $$"""
                DECLARE @TotalCount INT;

                -- 📌 Count query
                SELECT @TotalCount = COUNT(*)
                FROM [Project].[ProjectWorkBreakdownStructure] w
                INNER JOIN [Project].[ProjectMaster] p ON w.ProjectId = p.Id
                WHERE w.IsDeleted = 0
                AND p.IsDeleted = 0
                {{(string.IsNullOrWhiteSpace(searchTerm) ? "" : "AND (w.WorkBreakdownStructureName LIKE @Search OR p.ProjectCode LIKE @Search)")}};

                -- 📌 Data query
                SELECT 
                    w.Id,
                    w.ProjectId,
                    p.ProjectCode,
                    w.ParentWorkBreakdownStructureId,
                    pw.WorkBreakdownStructureName AS ParentWorkBreakdownStructureName,
                    w.WorkBreakdownStructureName,
                    w.WorkBreakdownStructureDescription,
                    w.StartDate,
                    w.EndDate,
                    w.DurationInDays,
                    w.ResponsibleDepartmentId,
                    w.ResponsiblePerson,
                    w.CostCenterId,
                    w.PlannedBudgetAmount,
                    w.CurrencyId,
                    w.IsMilestone,
                    w.MilestoneDate,
                    w.Remarks,
                    w.StatusId,
                    w.Level,
                    w.UnitId,
                    w.BudgetYearId
                FROM [Project].[ProjectWorkBreakdownStructure] w
                INNER JOIN [Project].[ProjectMaster] p ON w.ProjectId = p.Id
                LEFT JOIN [Project].[ProjectWorkBreakdownStructure] pw 
                    ON w.ParentWorkBreakdownStructureId = pw.Id
                WHERE w.IsDeleted = 0
                AND p.IsDeleted = 0
                {{(string.IsNullOrWhiteSpace(searchTerm) ? "" : "AND (w.WorkBreakdownStructureName LIKE @Search OR p.ProjectCode LIKE @Search)")}}
                ORDER BY w.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                -- 📌 Return total count
                SELECT @TotalCount AS TotalCount;
                """;

            var parameters = new
            {
                Search = string.IsNullOrWhiteSpace(searchTerm)
                    ? null
                    : $"%{searchTerm.Trim()}%",
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            using var multi = await _dbConnection.QueryMultipleAsync(query, parameters);

            var items = (await multi.ReadAsync<ProjectWorkBreakdownStructureDto>()).ToList();
            var totalCount = await multi.ReadFirstAsync<int>();

            return (items, totalCount);
        }

        //    public async Task<(IReadOnlyList<ProjectWorkBreakdownStructureDto> Items, int TotalCount)>
        //         GetAllAsync(int pageNumber, int pageSize, string? searchTerm)
        //     {
        //         if (pageNumber <= 0) pageNumber = 1;
        //         if (pageSize <= 0) pageSize = 20;

        //         var offset = (pageNumber - 1) * pageSize;

        //         // Base WHERE clause (soft delete)
        //         var whereClause = "WHERE w.IsDelete = 0";

        //         if (!string.IsNullOrWhiteSpace(searchTerm))
        //         {
        //             whereClause +=
        //                 " AND (w.WorkBreakdownStructureName LIKE @SearchPattern OR p.ProjectCode LIKE @SearchPattern)";
        //         }

        //         var countSql = $@"
        //             SELECT COUNT(1)
        //             FROM [Project].[ProjectWorkBreakdownStructure] w
        //             INNER JOIN [Project].[ProjectMaster] p ON w.ProjectId = p.Id
        //             {whereClause};
        //         ";

        //         var dataSql = $@"
        //             SELECT 
        //                 w.Id,
        //                 w.ProjectId,
        //                 p.ProjectCode,
        //                 w.ParentWorkBreakdownStructureId,
        //                 pw.WorkBreakdownStructureName AS ParentWorkBreakdownStructureName,
        //                 w.WorkBreakdownStructureName,
        //                 w.WorkBreakdownStructureDescription,
        //                 w.StartDate,
        //                 w.EndDate,
        //                 w.DurationInDays,
        //                 w.ResponsibleDepartmentId,
        //                 w.ResponsiblePerson,
        //                 w.CostCenterId,
        //                 w.PlannedBudgetAmount,
        //                 w.CurrencyId,
        //                 w.IsMilestone,
        //                 w.MilestoneDate,
        //                 w.Remarks,
        //                 w.StatusId,
        //                 w.Level,
        //                 w.UnitId,
        //                 w.BudgetYearId
        //             FROM [Project].[ProjectWorkBreakdownStructure] w
        //             INNER JOIN [Project].[ProjectMaster] p ON w.ProjectId = p.Id
        //             LEFT JOIN [Project].[ProjectWorkBreakdownStructure] pw 
        //                 ON w.ParentWorkBreakdownStructureId = pw.Id
        //             {whereClause}
        //             ORDER BY w.Id DESC
        //             OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
        //         ";

        //         var param = new
        //         {
        //             Offset = offset,
        //             PageSize = pageSize,
        //             SearchPattern = string.IsNullOrWhiteSpace(searchTerm)
        //                 ? null
        //                 : $"%{searchTerm.Trim()}%"
        //         };

        //         // Execute both queries separately
        //         var totalCount = await _dbConnection.ExecuteScalarAsync<int>(countSql, param);

        //         var items = (await _dbConnection.QueryAsync<ProjectWorkBreakdownStructureDto>(dataSql, param))
        //             .ToList();

        //         return (items, totalCount);
        //     }

        public async Task<IReadOnlyList<ProjectWorkBreakdownStructureDto>> GetByProjectAsync(int projectId)
        {
            const string sql = @"
                SELECT 
                    w.Id,
                    w.ProjectId,
                    p.ProjectCode,
                    w.ParentWorkBreakdownStructureId,
                    pw.WorkBreakdownStructureName AS ParentWorkBreakdownStructureName,
                    w.WorkBreakdownStructureName,
                    w.WorkBreakdownStructureDescription,
                    w.StartDate,
                    w.EndDate,
                    w.DurationInDays,
                    w.ResponsibleDepartmentId,
                    w.ResponsiblePerson,
                    w.CostCenterId,
                    w.PlannedBudgetAmount,
                    w.CurrencyId,
                    w.IsMilestone,
                    w.MilestoneDate,
                    w.Remarks,
                    w.StatusId,
                    w.Level,
                    w.UnitId,
                    w.BudgetYearId
                FROM [Project].[ProjectWorkBreakdownStructure] w
                INNER JOIN [Project].[ProjectMaster] p
                    ON w.ProjectId = p.Id
                LEFT JOIN [Project].[ProjectWorkBreakdownStructure] pw
                    ON w.ParentWorkBreakdownStructureId = pw.Id
                WHERE w.IsDeleted = 0
                  AND w.ProjectId = @ProjectId
                ORDER BY w.Level, w.WorkBreakdownStructureName;
            ";

            var result = await _dbConnection.QueryAsync<ProjectWorkBreakdownStructureDto>(
                sql,
                new { ProjectId = projectId });

            return result.ToList();
        }

        public async Task<ProjectWorkBreakdownStructureDto?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT 
                    w.Id,
                    w.ProjectId,
                    p.ProjectCode,
                    w.ParentWorkBreakdownStructureId,
                    pw.WorkBreakdownStructureName AS ParentWorkBreakdownStructureName,
                    w.WorkBreakdownStructureName,
                    w.WorkBreakdownStructureDescription,
                    w.StartDate,
                    w.EndDate,
                    w.DurationInDays,
                    w.ResponsibleDepartmentId,
                    w.ResponsiblePerson,
                    w.CostCenterId,
                    w.PlannedBudgetAmount,
                    w.CurrencyId,
                    w.IsMilestone,
                    w.MilestoneDate,
                    w.Remarks,
                    w.StatusId,
                    w.Level,
                    w.UnitId,
                    w.BudgetYearId
                FROM [Project].[ProjectWorkBreakdownStructure] w
                INNER JOIN [Project].[ProjectMaster] p
                    ON w.ProjectId = p.Id
                LEFT JOIN [Project].[ProjectWorkBreakdownStructure] pw
                    ON w.ParentWorkBreakdownStructureId = pw.Id
                WHERE w.IsDeleted = 0
                  AND w.Id = @Id;
            ";

            return await _dbConnection.QueryFirstOrDefaultAsync<ProjectWorkBreakdownStructureDto>(
                sql,
                new { Id = id });
        }
        public async Task<IReadOnlyList<ProjectWorkBreakdownStructureAutocompleteDto>> GetAutocompleteAsync(int? projectId, string? searchPattern)
        {
            var sql = @"
                SELECT TOP (20)
                    w.Id,
                    w.WorkBreakdownStructureName
                FROM [Project].[ProjectWorkBreakdownStructure] w
                WHERE w.IsDeleted = 0
            ";

            // Optional project filter
            if (projectId.HasValue && projectId.Value > 0)
            {
                sql += " AND w.ProjectId = @ProjectId";
            }

            // Optional search filter
            if (!string.IsNullOrWhiteSpace(searchPattern))
            {
                sql += " AND w.WorkBreakdownStructureName LIKE @SearchPattern";
            }

            sql += " ORDER BY w.WorkBreakdownStructureName;";

            var result = await _dbConnection.QueryAsync<ProjectWorkBreakdownStructureAutocompleteDto>(
                sql,
                new
                {
                    ProjectId = projectId,
                    SearchPattern = string.IsNullOrWhiteSpace(searchPattern)
                        ? null
                        : $"%{searchPattern.Trim()}%"
                });

            return result.ToList();
        }


        // public async Task<IReadOnlyList<ProjectWorkBreakdownStructureAutocompleteDto>> GetAutocompleteAsync(
        //     int? projectId, string? searchPattern)
        // {
        //     var sql = @"
        //         SELECT TOP (20)
        //             w.Id,
        //             w.WorkBreakdownStructureName
        //         FROM [Project].[ProjectWorkBreakdownStructure] w
        //         WHERE w.IsDeleted = 0
        //           AND w.ProjectId = @ProjectId
        //     ";

        //     if (!string.IsNullOrWhiteSpace(searchPattern))
        //     {
        //         sql += " AND w.WorkBreakdownStructureName LIKE '%' + @SearchPattern + '%'";
        //     }

        //     sql += " ORDER BY w.WorkBreakdownStructureName;";

        //     var result = await _dbConnection.QueryAsync<ProjectWorkBreakdownStructureAutocompleteDto>(
        //         sql,
        //         new
        //         {
        //             ProjectId = projectId ?? 0,
        //             SearchPattern = searchPattern?.Trim()
        //         });

        //     return result.ToList();
        // }

        public async Task<bool> IsNameUniqueAsync(int projectId, string wbsName, int? excludeId = null)
        {
            var sql = @"
                SELECT COUNT(1)
                FROM [Project].[ProjectWorkBreakdownStructure] w
                WHERE w.IsDeleted = 0
                  AND w.ProjectId = @ProjectId
                  AND w.WorkBreakdownStructureName = @Name
            ";

            if (excludeId.HasValue)
            {
                sql += " AND w.Id <> @ExcludeId";
            }

            var count = await _dbConnection.ExecuteScalarAsync<int>(
                sql,
                new
                {
                    ProjectId = projectId,
                    Name = wbsName.Trim(),
                    ExcludeId = excludeId
                });

            return count == 0;
        }

        public async Task<int> GetParentLevelAsync(int parentId)
        {
            const string sql = @"
                SELECT TOP (1) [Level]
                FROM [Project].[ProjectWorkBreakdownStructure]
                WHERE IsDeleted = 0
                  AND Id = @ParentId;
            ";

            var level = await _dbConnection.ExecuteScalarAsync<int?>(sql, new { ParentId = parentId });

            // if parent not found or level is null, default to 1
            return level.GetValueOrDefault(1);
        }
        
                public async Task<List<ProjectWbsLookupDto>> GetWbsLookupAsync(int? projectId = null, CancellationToken ct = default)
            {
                var unitId = _iPAddressService.GetUnitId();

                const string sql = @"
                    SELECT Id,
                        ParentWorkBreakdownStructureId,
                        WorkBreakdownStructureName
                    FROM [Project].[ProjectWorkBreakdownStructure]
                    WHERE IsDeleted = 0
                    AND IsActive = 1
                    AND ParentWorkBreakdownStructureId IS NULL
                    AND UnitId = @UnitId
                    AND (@ProjectId IS NULL OR ProjectId = @ProjectId)
                    ORDER BY WorkBreakdownStructureName;
                ";

                var result = await _dbConnection.QueryAsync<ProjectWbsLookupDto>(
                    new CommandDefinition(
                        sql,
                        new { UnitId = unitId, ProjectId = projectId },
                        cancellationToken: ct
                    )
                );

                return result.ToList();
            }
            
    }
}