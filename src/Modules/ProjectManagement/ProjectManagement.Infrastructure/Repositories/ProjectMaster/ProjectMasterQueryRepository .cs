using System.Data;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces;
using ProjectManagement.Application.Common.Interfaces;
using ProjectManagement.Application.Common.Interfaces.IMiscMaster;
using ProjectManagement.Application.Common.Interfaces.IProjectMaster;
using ProjectManagement.Application.ProjectMaster.Queries.GetProjectMaster;
using ProjectManagement.Application.ProjectMaster.Queries.GetProjectPendingApprovals;
using ProjectManagement.Application.ProjectMaster.Queries.ProjectMasterAutoComplete;
using ProjectManagement.Domain.Common;
using Dapper;

namespace ProjectManagement.Infrastructure.Repositories.ProjectMaster
{
    public class ProjectMasterQueryRepository : IProjectMasterQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IIPAddressService _iPAddressService;
        private readonly IDepartmentLookup _departmentLookup;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;

        public ProjectMasterQueryRepository(IDbConnection dbConnection, IIPAddressService iPAddressService, IDepartmentLookup departmentLookup, IMiscMasterQueryRepository miscMasterQueryRepository)
        {
            _dbConnection = dbConnection;
            _iPAddressService = iPAddressService;
            _departmentLookup = departmentLookup;
            _miscMasterQueryRepository = miscMasterQueryRepository  ;
        }

        public async Task<(IReadOnlyList<GetProjectMasterDto> Items, int TotalCount)> GetProjectmasterAsync(
         int pageNumber,
         int pageSize,
         string? searchTerm,
         CancellationToken ct = default)
        {
            var unitId = _iPAddressService.GetUnitId() ?? 0;



            var query = $$"""
                DECLARE @TotalCount INT;

                SELECT @TotalCount = COUNT(*) 
                FROM [Project].[ProjectMaster] P
                WHERE P.IsDeleted = 0
                AND P.UnitId = @UnitId
                {{(string.IsNullOrEmpty(searchTerm) ? "" : "AND (P.ProjectCode LIKE @Search OR P.ProjectName LIKE @Search)")}};

                SELECT 
                    P.Id,
                    P.ProjectCode,
                    P.ProjectName,
                    P.ProjectDescription,
                    P.ProjectTypeId,
                    pt.Code AS ProjectType,
                    P.UnitId,
                    P.DepartmentId,
                    P.BudgetAmount,
                    P.BudgetYearId,
                    P.CostCenterId,
                    P.CurrencyId,
                    P.StartDate,
                    P.EndDate,
                    P.ProjectCategoryId,
                    pc.Code as ProjectCategory,
                    P.AssetGroupId,
                    P.PurposeRemarks,
                    P.IsActive,
                    P.IsDeleted,
                    P.CreatedBy,
                    P.CreatedDate,
                    P.CreatedByName,
                    P.CreatedIP,
                    P.ModifiedBy,
                    P.ModifiedDate,
                    P.ModifiedByName,
                    P.ModifiedIP,
                    P.StatusId
                FROM [Project].[ProjectMaster] P                 
                LEFT JOIN Project.MiscMaster pc ON p.ProjectCategoryId = pc.Id
                LEFT JOIN Project.MiscMaster pt ON p.ProjectTypeId = pt.Id
                LEFT JOIN Project.MiscMaster st ON p.StatusId = st.Id
                WHERE P.IsDeleted = 0
                AND P.UnitId = @UnitId
               
                {{(string.IsNullOrEmpty(searchTerm) ? "" : "AND (P.ProjectCode LIKE @Search OR P.ProjectName LIKE @Search)")}}
                ORDER BY P.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;
            """;

            var parameters = new
            {
                UnitId = unitId,

                Search = $"%{searchTerm}%",
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            using var multi = await _dbConnection.QueryMultipleAsync(query, parameters);
            var projectMasters = (await multi.ReadAsync<GetProjectMasterDto>()).ToList();
            var totalCount = await multi.ReadFirstAsync<int>();

            // Load documents for each project
            if (projectMasters.Count > 0)
            {
                var projectIds = projectMasters.Select(p => p.Id).ToArray();

                const string docsSql = @"
                    SELECT 
                        d.Id,
                        d.ProjectId,
                        d.DocumentId,
                        mm.Code AS DocumentName,
                        d.FileName,
                        d.UploadedDate,        
                        CONCAT(            
                            CASE 
                                WHEN RIGHT(ISNULL(mtm.Description, ''), 1) = '/' 
                                    THEN ISNULL(mtm.Description, '')
                                ELSE ISNULL(mtm.Description, '') + '/'
                            END,            
                            CASE 
                                WHEN LEFT(ISNULL(mt.Description, ''), 1) = '/' 
                                    THEN SUBSTRING(ISNULL(mt.Description, ''), 2, LEN(ISNULL(mt.Description, '')))
                                ELSE ISNULL(mt.Description, '')
                            END,
                            '/',            
                            REPLACE(d.FileName, '\', '/')
                        ) AS UploadedPath
                    FROM Project.ProjectDocument AS d WITH (NOLOCK)
                    JOIN Project.MiscMaster AS mm ON mm.Id = d.DocumentId
                    LEFT JOIN Project.MiscTypeMaster mtm ON mtm.MiscTypeCode = 'ImagePath'
                    LEFT JOIN Project.MiscTypeMaster mt ON mt.MiscTypeCode = 'ProjectDocument'
                    WHERE d.ProjectId IN @ProjectIds
                    ORDER BY d.Id;";

                var rows = await _dbConnection.QueryAsync<GetProjectDocumentDto>(
                    new CommandDefinition(docsSql, new { ProjectIds = projectIds }, cancellationToken: ct));

                var docsByProject = rows
                    .GroupBy(x => x.ProjectId)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(r => new GetProjectDocumentDto
                        {
                            Id = r.Id,
                            ProjectId = r.ProjectId,
                            DocumentId = r.DocumentId,
                            FileName = r.FileName,
                            UploadedDate = r.UploadedDate,
                            UploadedPath = r.UploadedPath
                        }).ToList());

                // Assign documents to each ProjectMasterDto
                foreach (var pm in projectMasters)
                {
                    pm.Documents = docsByProject.ContainsKey(pm.Id)
                        ? docsByProject[pm.Id]
                        : new List<GetProjectDocumentDto>();
                }
            }

            return (projectMasters, totalCount);
        }
  
        public async Task<GetProjectMasterDto?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var unitId = _iPAddressService.GetUnitId() ?? 0;
            const string sqlProject = @"
                  SELECT 
                    p.Id,
                    p.ProjectCode,
                    p.ProjectName,
                    p.ProjectDescription,
                    p.ProjectTypeId,
					pt.Code AS ProjectType,
                    p.UnitId,
                    p.DepartmentId,
                    p.BudgetAmount,
                    p.BudgetYearId,
                    p.CostCenterId,
                    p.CurrencyId,
                    p.StartDate,
                    p.EndDate,
                    p.ProjectCategoryId,
					pc.Code as ProjectCategory,
                    p.AssetGroupId,
                    p.PurposeRemarks
                FROM Project.ProjectMaster p
				left join project.MiscMaster pc on p.ProjectCategoryId=pc.Id
				left join project.MiscMaster pt on p.ProjectTypeId=pt.Id
                Left join project.MiscMaster st on p.StatusId=st.Id                
                WHERE p.Id = @Id AND p.IsDeleted = 0
                AND (@UnitId IS NULL OR UnitId = @UnitId);";

            const string sqlDocs = @"
                SELECT 
                    d.Id,
                    d.ProjectId,

                    d.DocumentId,
                    mm.Code AS DocumentName,
                    d.FileName,
                    d.UploadedDate,        
                    CONCAT(            
                        CASE 
                            WHEN RIGHT(ISNULL(mtm.Description, ''), 1) = '/' 
                                THEN ISNULL(mtm.Description, '')
                            ELSE ISNULL(mtm.Description, '') + '/'
                        END,            
                        CASE 
                            WHEN LEFT(ISNULL(mt.Description, ''), 1) = '/' 
                                THEN SUBSTRING(ISNULL(mt.Description, ''), 2, LEN(ISNULL(mt.Description, '')))
                            ELSE ISNULL(mt.Description, '')
                        END,
                        '/',            
                        REPLACE(d.FileName, '\', '/')
                    ) AS UploadedPath
                FROM Project.ProjectDocument AS d WITH (NOLOCK)
                JOIN Project.MiscMaster AS mm ON mm.Id = d.DocumentId
                LEFT JOIN Project.MiscTypeMaster mtm ON mtm.MiscTypeCode = 'ImagePath'
                LEFT JOIN Project.MiscTypeMaster mt ON mt.MiscTypeCode = 'ProjectDocument'
                WHERE d.ProjectId = @Id
                ORDER BY d.Id;";

            // single project
            var project = await _dbConnection.QueryFirstOrDefaultAsync<GetProjectMasterDto>(
                new CommandDefinition(sqlProject, new { Id = id, UnitId = unitId }, cancellationToken: ct));

            if (project == null)
                return null;

            // documents
            var docs = await _dbConnection.QueryAsync<GetProjectDocumentDto>(
                new CommandDefinition(sqlDocs, new { Id = id }, cancellationToken: ct));

            project.Documents = docs.ToList();
            return project;
        }
        
      public async Task<List<ProjectMasterAutoCompleteDto>> GetProjectMasterAutoCompleteAsync(
            int? unitId,
            int? departmentId,
            string? searchTerm,
            int take,
            CancellationToken ct = default)
        {
            if (take <= 0) take = 100;

            var term = searchTerm?.Trim();

            // Treat searchTerm as status ONLY if it matches known status words
            string? statusText = null;
            string? searchText = null;

            if (!string.IsNullOrWhiteSpace(term))
            {
                // exact-match status keywords
                if (term.Equals(MiscEnumEntity.Approved, StringComparison.OrdinalIgnoreCase) ||
                    term.Equals(MiscEnumEntity.Pending, StringComparison.OrdinalIgnoreCase) ||
                    term.Equals(MiscEnumEntity.Rejected, StringComparison.OrdinalIgnoreCase) ||
                    term.Equals(MiscEnumEntity.Draft, StringComparison.OrdinalIgnoreCase) ||
                    term.Equals(MiscEnumEntity.Open, StringComparison.OrdinalIgnoreCase) ||
                    term.Equals(MiscEnumEntity.Deleted, StringComparison.OrdinalIgnoreCase))
                {
                    statusText = term;     // e.g. "Approved"
                    searchText = null;     // ✅ don't do LIKE search with "Approved"
                }
                else
                {
                    searchText = term;     // normal text search
                }
            }

            // Resolve StatusId only when searchTerm is a status keyword
            int? statusId = null;
            if (!string.IsNullOrWhiteSpace(statusText))
            {
                var misc = await _miscMasterQueryRepository.GetMiscMasterByName(
                    MiscEnumEntity.ApprovalStatus,  // group/entity (ex: "ApprovalStatus")
                    statusText                      // value (ex: "Approved")
                );

                statusId = misc?.Id;

                // status text provided but not found in Misc -> return empty
                if (statusId is null)
                    return new List<ProjectMasterAutoCompleteDto>();
            }

            const string sql = @"
                SELECT 
                    Id,
                    ProjectCode,
                    ProjectName,
                    ProjectDescription,
                    UnitId,
                    DepartmentId,
                    ProjectTypeId,
                    BudgetAmount,
                    BudgetYearId,
                    CostCenterId,
                    CurrencyId,
                    StartDate,
                    EndDate,
                    ProjectCategoryId,
                    AssetGroupId,
                    PurposeRemarks,
                    StatusId
                FROM Project.ProjectMaster WITH (NOLOCK)
                WHERE IsDeleted = 0
                AND IsActive = 1
                AND (@UnitId IS NULL OR UnitId = @UnitId)
                AND (@DepartmentId IS NULL OR DepartmentId = @DepartmentId)
                AND (@StatusId IS NULL OR StatusId = @StatusId)
                AND (
                        @SearchText IS NULL OR @SearchText = ''
                        OR ProjectCode LIKE '%' + @SearchText + '%'
                        OR ProjectName LIKE '%' + @SearchText + '%'
                        OR ProjectDescription LIKE '%' + @SearchText + '%'
                )
                ORDER BY ProjectName;";

            var parameters = new
            {
                UnitId = unitId,
                DepartmentId = departmentId,
                Take = take,
                StatusId = statusId,
                SearchText = searchText
            };

            var result = await _dbConnection.QueryAsync<ProjectMasterAutoCompleteDto>(
                new CommandDefinition(sql, parameters, cancellationToken: ct));

            return result.AsList();
        }

        public async Task<string?> GetProjectNameAsync(int projectId, CancellationToken ct = default)
        {
            if (projectId <= 0)
                return null;

            var project = await GetByIdAsync(projectId, ct);
            return project?.ProjectName;
        }
        
        public async Task<(List<GetProjectPendingApprovalDto> Rows, int TotalCount)> GetProjectPendingAsync(
    int pageNumber,
    int pageSize,
    string? searchTerm,
    int? projectId,
    int? departmentId,
    int? projectTypeId,
    int? budgetYearId,
    int unitId,
    int pendingStatusId,
    CancellationToken ct = default)
        {
            var p = pageNumber > 0 ? pageNumber : 1;
            var s = pageSize > 0 ? pageSize : 15;
            var offset = (p - 1) * s;

            var likeSearch = string.IsNullOrWhiteSpace(searchTerm) ? null : $"%{searchTerm.Trim()}%";

            const string sql = @"
                SET NOCOUNT ON;

                CREATE TABLE #filtered (Id INT PRIMARY KEY);

                INSERT INTO #filtered (Id)
                SELECT DISTINCT PM.Id
                FROM Project.ProjectMaster PM
                WHERE PM.IsDeleted = 0
                AND PM.UnitId = @UnitId
                AND PM.StatusId = @PendingStatusId
                AND (@ProjectId IS NULL OR PM.Id = @ProjectId)
                AND (@DepartmentId IS NULL OR PM.DepartmentId = @DepartmentId)
                AND (@ProjectTypeId IS NULL OR PM.ProjectTypeId = @ProjectTypeId)
                AND (@BudgetYearId IS NULL OR PM.BudgetYearId = @BudgetYearId)
                AND (
                        @LikeSearch IS NULL OR
                        PM.ProjectCode LIKE @LikeSearch OR
                        PM.ProjectName LIKE @LikeSearch
                );

                CREATE TABLE #paged (Id INT PRIMARY KEY);

                ;WITH numbered AS (
                    SELECT Id, ROW_NUMBER() OVER (ORDER BY Id DESC) AS rn
                    FROM #filtered
                )
                INSERT INTO #paged (Id)
                SELECT Id
                FROM numbered
                WHERE rn BETWEEN (@Offset + 1) AND (@Offset + @PageSize);

                SELECT
                    PM.Id AS Id,
                    PM.ProjectCode,
                    PM.ProjectName,
                    PM.ProjectDescription,
                    PM.ProjectTypeId,
                    PM.UnitId,
                    PM.DepartmentId,
                    PM.BudgetAmount,
                    PM.BudgetYearId,
                    PM.CostCenterId,
                    PM.CurrencyId,
                    PM.StartDate,
                    PM.EndDate,
                    PM.StatusId,
                    MS.Code AS Status
                FROM Project.ProjectMaster PM
                INNER JOIN #paged p ON p.Id = PM.Id
                LEFT JOIN Project.MiscMaster MS ON MS.Id = PM.StatusId
                ORDER BY PM.Id DESC;

                SELECT COUNT(1) FROM #filtered;

                DROP TABLE #paged;
                DROP TABLE #filtered;
            ";

            var param = new
            {
                UnitId = unitId,
                PendingStatusId = pendingStatusId,
                ProjectId = projectId,
                DepartmentId = departmentId,
                ProjectTypeId = projectTypeId,
                BudgetYearId = budgetYearId,
                LikeSearch = likeSearch,
                Offset = offset,
                PageSize = s
            };

            using var multi = await _dbConnection.QueryMultipleAsync(
                new CommandDefinition(sql, param, cancellationToken: ct));

            var rows = (await multi.ReadAsync<GetProjectPendingApprovalDto>()).ToList();
            var total = await multi.ReadFirstAsync<int>();

            return (rows, total);
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string query = "SELECT COUNT(1) FROM [Project].[ProjectMaster] WHERE Id = @id AND IsDeleted = 0";
            var count = await _dbConnection.ExecuteScalarAsync<int>(query, new { id });
            return count == 0;
        }

        public async Task<bool> SoftDeleteValidationAsync(int id)
        {
            const string query = @"
                SELECT CASE WHEN
                    EXISTS (SELECT 1 FROM [Project].[ProjectDocument] WHERE ProjectId = @id)
                    OR
                    EXISTS (SELECT 1 FROM [Project].[ProjectWorkBreakdownStructure] WHERE ProjectId = @id AND IsDeleted = 0)
                THEN 1 ELSE 0 END;";

            return await _dbConnection.ExecuteScalarAsync<bool>(query, new { id });
        }
    }
}
