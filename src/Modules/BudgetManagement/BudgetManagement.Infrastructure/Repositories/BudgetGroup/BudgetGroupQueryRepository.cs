using System.Data;
using BudgetManagement.Application.BudgetGroups;
using BudgetManagement.Application.Common.Interfaces.IBudgetGroupMaster;
using Dapper;
using BudgetManagement.Domain.Common;
using Contracts.Interfaces;
using BudgetManagement.Application.Common.Interfaces;

namespace BudgetManagement.Infrastructure.Repositories.BudgetGroup
{
    public class BudgetGroupQueryRepository : IBudgetGroupQueryRepository
    {
        private readonly IDbConnection _db;
        private readonly IIPAddressService _ipAddressService;

        public BudgetGroupQueryRepository(IDbConnection db, IIPAddressService ipAddressService)
        {
            _db = db;
            _ipAddressService = ipAddressService;
        }

        //  LIST 
        public async Task<(List<BudgetGroupListItemDto> Items, int TotalCount)> GetAllAsync(
            BudgetGroupListFilterDto filter,
            CancellationToken ct = default)
        {

            var skip = (filter.PageNumber - 1) * filter.PageSize;
            var UnitId = _ipAddressService.GetUnitId() ?? 0;

const string sql = @"
    SELECT 
        bg.Id,
        bg.Name,
        bg.Description,
        bg.UnitId,
        bg.DepartmentId,
        bg.CostCenterId,
        bg.ParentBudgetGroupId,
        CASE 
            WHEN bg.IsParent = 1 THEN bg.Name
            ELSE pbg.Name
        END AS ParentBudgetGroupName,
        bg.CurrencyId,
        CAST(NULL AS NVARCHAR(200)) AS CurrencyName,  
        CASE WHEN arT.Id IS NOT NULL THEN bg.AllocationRuleId ELSE NULL END AS AllocationRuleId,
        CASE WHEN arT.Id IS NOT NULL THEN ar.Description ELSE NULL END AS AllocationRuleName,
        bg.AllocatedPercentage,
        bg.AllocatedSpindleCost,
        bg.BudgetTypeId,
        CASE WHEN btT.Id IS NOT NULL THEN bt.Description ELSE NULL END AS BudgetTypeName,
        bg.CarryForward,
        bg.IsParent,
        bg.IsActive
    FROM [Budget].[BudgetGroup] bg WITH (NOLOCK)
    LEFT JOIN [Budget].[BudgetGroup] pbg WITH (NOLOCK) 
        ON pbg.Id = bg.ParentBudgetGroupId AND pbg.IsDeleted = 0
    LEFT JOIN [Budget].[MiscMaster] ar WITH (NOLOCK)
        ON ar.Id = bg.AllocationRuleId AND ar.IsDeleted = 0
    LEFT JOIN [Budget].[MiscTypeMaster] arT WITH (NOLOCK)
        ON arT.Id = ar.MiscTypeId AND arT.IsDeleted = 0
        AND LOWER(arT.MiscTypeCode) = LOWER(@AllocationTypeMiscTypeCode)
    LEFT JOIN [Budget].[MiscMaster] bt WITH (NOLOCK)
    ON bt.Id = bg.BudgetTypeId AND bt.IsDeleted = 0
    LEFT JOIN [Budget].[MiscTypeMaster] btT WITH (NOLOCK)
    ON btT.Id = bt.MiscTypeId AND btT.IsDeleted = 0
    AND LOWER(btT.MiscTypeCode) = LOWER(@BudgetTypeMiscTypeCode)
    WHERE 
        bg.IsDeleted = 0
        AND (@SearchTerm IS NULL OR bg.Name LIKE @SearchTerm)
        AND (@UnitId IS NULL OR bg.UnitId = @UnitId)
        AND (@DepartmentId IS NULL OR bg.DepartmentId = @DepartmentId)
        AND (@CostCenterId IS NULL OR bg.CostCenterId = @CostCenterId)
        AND (@ParentBudgetGroupId IS NULL OR bg.ParentBudgetGroupId = @ParentBudgetGroupId)
        AND (@BudgetTypeId IS NULL OR bg.BudgetTypeId = @BudgetTypeId)
        AND (@CarryForward IS NULL OR bg.CarryForward = @CarryForward)
        AND (@AllocationRuleId IS NULL OR bg.AllocationRuleId = @AllocationRuleId)
        AND (
            @IsActive IS NULL
            OR (bg.IsActive = 1 AND @IsActive = 1)
            OR (bg.IsActive = 0 AND @IsActive = 0)
        )
    ORDER BY bg.Id DESC
    OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;

    -- Get the total count
    SELECT COUNT(1) AS TotalCount
    FROM [Budget].[BudgetGroup] bg WITH (NOLOCK)
    WHERE 
        bg.IsDeleted = 0
        AND (@SearchTerm IS NULL OR bg.Name LIKE @SearchTerm)
        AND (@UnitId IS NULL OR bg.UnitId = @UnitId)
        AND (@DepartmentId IS NULL OR bg.DepartmentId = @DepartmentId)
        AND (@CostCenterId IS NULL OR bg.CostCenterId = @CostCenterId)
        AND (@ParentBudgetGroupId IS NULL OR bg.ParentBudgetGroupId = @ParentBudgetGroupId)
        AND (@BudgetTypeId IS NULL OR bg.BudgetTypeId = @BudgetTypeId)
        AND (@CarryForward IS NULL OR bg.CarryForward = @CarryForward)
        AND (@AllocationRuleId IS NULL OR bg.AllocationRuleId = @AllocationRuleId)
        AND (
            @IsActive IS NULL
            OR (bg.IsActive = 1 AND @IsActive = 1)
            OR (bg.IsActive = 0 AND @IsActive = 0)
        );";

            var param = new
            {
                SearchTerm = string.IsNullOrWhiteSpace(filter.SearchTerm)? null : $"%{filter.SearchTerm.Trim()}%",
                UnitId,
                DepartmentId = filter.DepartmentId,
                CostCenterId = filter.CostCenterId,
                ParentBudgetGroupId = filter.ParentBudgetGroupId,
                AllocationRuleId = filter.AllocationRuleId,
                BudgetTypeId = filter.BudgetTypeId,
                CarryForward = filter.CarryForward,
                BudgetTypeMiscTypeCode = MiscEnumEntity.BudgetType,
                IsActive = filter.IsActive,
                Skip = skip,
                Take = filter.PageSize,
                AllocationTypeMiscTypeCode = MiscEnumEntity.AllocationType
            };

            using var multi = await _db.QueryMultipleAsync(sql, param);

            var rawItems = (await multi.ReadAsync<dynamic>()).ToList();
            var total = await multi.ReadSingleAsync<int>();

            var items = rawItems.Select(x => new BudgetGroupListItemDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,

                UnitId = x.UnitId,
                DepartmentId = x.DepartmentId,
                CostCenterId = x.CostCenterId,

                ParentBudgetGroupId = x.ParentBudgetGroupId,
                ParentBudgetGroupName = x.ParentBudgetGroupName,

                CurrencyId = x.CurrencyId,
                CurrencyName = x.CurrencyName,

                AllocationRuleId = x.AllocationRuleId,
                AllocationRuleName = x.AllocationRuleName,

                AllocatedPercentage = x.AllocatedPercentage,
                AllocatedSpindleCost = x.AllocatedSpindleCost,

                BudgetTypeId = x.BudgetTypeId,
                BudgetTypeName = x.BudgetTypeName,
                CarryForward = x.CarryForward,

                UnitName = string.Empty,
                DepartmentName = string.Empty,
                CostCenterName = string.Empty,

                IsParent = x.IsParent,
                IsActive = x.IsActive
            }).ToList();

            return (items, total);
        }



        //  GET BY ID
        public async Task<BudgetGroupDto?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            const string sql = @"
            SELECT 
            bg.Id,
            bg.Name,
            bg.Description,
            bg.UnitId,
            bg.DepartmentId,
            bg.CostCenterId,
            bg.ParentBudgetGroupId,
            bg.BudgetTypeId,
            bt.Description AS BudgetTypeName,
            bg.CarryForward,
            CASE 
                WHEN bg.IsParent = 1 THEN bg.Name 
                ELSE parent.Name 
            END AS ParentBudgetGroupName,
            bg.CurrencyId,
            bg.AllocationRuleId,
            CASE 
            WHEN ar.Id IS NOT NULL THEN ar.Description 
            ELSE 'No Allocation Rule'  
            END AS AllocationRuleName,
            bg.AllocatedPercentage,
            bg.AllocatedSpindleCost,
            bg.IsParent,
            bg.IsActive,
            bg.IsDeleted
            FROM [Budget].[BudgetGroup] bg WITH (NOLOCK)
            LEFT JOIN Budget.BudgetGroup parent WITH (NOLOCK)
            ON parent.Id = bg.ParentBudgetGroupId
            LEFT JOIN Budget.MiscMaster ar WITH (NOLOCK)
            ON ar.Id = bg.AllocationRuleId AND ar.IsDeleted = 0
            LEFT JOIN Budget.MiscMaster bt WITH (NOLOCK)
            ON bt.Id = bg.BudgetTypeId AND bt.IsDeleted = 0
            LEFT JOIN Budget.MiscTypeMaster btT WITH (NOLOCK)
            ON btT.Id = bt.MiscTypeId AND btT.IsDeleted = 0
            AND LOWER(btT.MiscTypeCode) = LOWER(@BudgetTypeMiscTypeCode)
            WHERE 
            bg.Id = @Id
            AND bg.IsDeleted = 0;";

            var param = new { Id = id, BudgetTypeMiscTypeCode = MiscEnumEntity.BudgetType };
            var entity = await _db.QueryFirstOrDefaultAsync<dynamic>(sql, param);

            if (entity == null)
                return null;

            var dto = new BudgetGroupDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,

                UnitId = entity.UnitId,
                UnitName = string.Empty,

                DepartmentId = entity.DepartmentId,
                DepartmentName = string.Empty,

                CostCenterId = entity.CostCenterId,
                CostCenterName = string.Empty,

                ParentBudgetGroupId = entity.ParentBudgetGroupId,
                ParentBudgetGroupName = entity.ParentBudgetGroupName,
                CurrencyId = entity.CurrencyId,

                AllocationRuleId = entity.AllocationRuleId,
                AllocationRuleName = entity.AllocationRuleName,

                AllocatedPercentage = entity.AllocatedPercentage,
                AllocatedSpindleCost = entity.AllocatedSpindleCost,

                BudgetTypeId = entity.BudgetTypeId,
                BudgetTypeName = entity.BudgetTypeName,
                CarryForward = entity.CarryForward,

                IsParent = entity.IsParent,
                IsActive = entity.IsActive
            };

            return dto;
        }

        // AUTOCOMPLETE 
        public async Task<List<BudgetGroupAutoCompleteDto>> GetBudgetGroupAutoCompleteAsync(
            string searchPattern,
            CancellationToken ct = default)
        {
            const string sql = @"
        SELECT 
        bg.Id,
        bg.Name,
        bg.ParentBudgetGroupId,
        bg.BudgetTypeId,
        CASE WHEN btT.Id IS NOT NULL THEN bt.Description ELSE NULL END AS BudgetTypeName,
        bg.CarryForward,
        CASE 
            WHEN bg.IsParent = 1 THEN bg.Name 
            ELSE NULL 
        END AS ParentBudgetGroupName,
        bg.IsParent,
        bg.AllocationRuleId,                
        ar.Description AS AllocationRuleName 
        FROM Budget.BudgetGroup bg WITH (NOLOCK)
        LEFT JOIN Budget.MiscMaster ar WITH (NOLOCK)
        ON ar.Id = bg.AllocationRuleId AND ar.IsDeleted = 0
        LEFT JOIN Budget.MiscMaster bt WITH (NOLOCK)
        ON bt.Id = bg.BudgetTypeId AND bt.IsDeleted = 0
        LEFT JOIN Budget.MiscTypeMaster btT WITH (NOLOCK)
        ON btT.Id = bt.MiscTypeId AND btT.IsDeleted = 0
        AND LOWER(btT.MiscTypeCode) = LOWER(@BudgetTypeMiscTypeCode)     
        WHERE 
        bg.IsActive = 1
        AND bg.IsDeleted = 0
        AND (@SearchPattern IS NULL OR bg.Name LIKE @SearchPattern)
        ORDER BY bg.IsParent DESC, bg.CreatedDate DESC;";

            var param = new
            {
                SearchPattern = string.IsNullOrWhiteSpace(searchPattern) ? null : $"%{searchPattern.Trim()}%",
                BudgetTypeMiscTypeCode = MiscEnumEntity.BudgetType
            };

            var list = await _db.QueryAsync<BudgetGroupAutoCompleteDto>(sql, param);

            return list.ToList(); 
        }

        // SOFT DELETE VALIDATION
        public async Task<bool> SoftDeleteValidation(int id, CancellationToken ct = default)
        {
            const string sql = @"
                SELECT CASE WHEN
                    EXISTS (SELECT 1 FROM [Budget].[BudgetGroup] WHERE ParentBudgetGroupId = @Id AND IsDeleted = 0)
                    OR EXISTS (SELECT 1 FROM [Budget].[BudgetAllocation] WHERE BudgetGroupId = @Id AND IsDeleted = 0)
                    OR EXISTS (SELECT 1 FROM [Budget].[BudgetRequest] WHERE BudgetGroupId = @Id AND IsDeleted = 0)
                THEN 1 ELSE 0 END;";

            var result = await _db.QueryFirstOrDefaultAsync<int>(sql, new { Id = id });
            return result == 1;
        }

        public async Task<bool> IsBudgetGroupLinkedAsync(int id, CancellationToken ct = default)
        {
            const string sql = @"
                SELECT CASE WHEN
                    EXISTS (SELECT 1 FROM [Budget].[BudgetGroup] WHERE ParentBudgetGroupId = @Id AND IsDeleted = 0 AND IsActive = 1)
                    OR EXISTS (SELECT 1 FROM [Budget].[BudgetAllocation] WHERE BudgetGroupId = @Id AND IsDeleted = 0 AND IsActive = 1)
                    OR EXISTS (SELECT 1 FROM [Budget].[BudgetRequest] WHERE BudgetGroupId = @Id AND IsDeleted = 0 AND IsActive = 1)
                THEN 1 ELSE 0 END;";

            var result = await _db.QueryFirstOrDefaultAsync<int>(sql, new { Id = id });
            return result == 1;
        }

        public async Task<List<BudgetGroupAutoCompleteDto>> GetBudgetGroupByDepartmentAsync(int departmentId, string? searchPattern, CancellationToken ct = default)
        {
            var unitId = _ipAddressService.GetUnitId() ?? 0;

            const string sql = @"
        SELECT 
        bg.Id,
        bg.Name,
        bg.ParentBudgetGroupId,
        bg.BudgetTypeId,
            CASE WHEN btT.Id IS NOT NULL THEN bt.Description ELSE NULL END AS BudgetTypeName,
        bg.CarryForward,
        CASE 
            WHEN bg.ParentBudgetGroupId IS NULL THEN NULL
            ELSE pbg.Name
        END AS ParentBudgetGroupName,
        bg.IsParent,
        bg.AllocationRuleId,
        ar.Description AS AllocationRuleName
        FROM Budget.BudgetGroup bg WITH (NOLOCK)
        LEFT JOIN Budget.BudgetGroup pbg WITH (NOLOCK)
        ON pbg.Id = bg.ParentBudgetGroupId AND pbg.IsDeleted = 0
        LEFT JOIN Budget.MiscMaster ar WITH (NOLOCK)
        ON ar.Id = bg.AllocationRuleId AND ar.IsDeleted = 0
        LEFT JOIN Budget.MiscMaster bt WITH (NOLOCK)
        ON bt.Id = bg.BudgetTypeId AND bt.IsDeleted = 0
        LEFT JOIN Budget.MiscTypeMaster btT WITH (NOLOCK)
        ON btT.Id = bt.MiscTypeId AND btT.IsDeleted = 0
        AND LOWER(btT.MiscTypeCode) = LOWER(@BudgetTypeMiscTypeCode)

        WHERE
        bg.IsDeleted = 0
        AND bg.IsActive = 1
        AND bg.UnitId = @UnitId
        AND bg.DepartmentId = @DepartmentId
        AND (@Search IS NULL OR bg.Name LIKE @Search)
        ORDER BY bg.IsParent DESC, bg.CreatedDate DESC, bg.Id DESC;";

            var param = new
            {
                UnitId = unitId,
                DepartmentId = departmentId,
                Search = string.IsNullOrWhiteSpace(searchPattern) ? null : $"%{searchPattern.Trim()}%",
                BudgetTypeMiscTypeCode = MiscEnumEntity.BudgetType
            };

            var list = await _db.QueryAsync<BudgetGroupAutoCompleteDto>(sql, param);
            return list.ToList();
        }
    }
}
