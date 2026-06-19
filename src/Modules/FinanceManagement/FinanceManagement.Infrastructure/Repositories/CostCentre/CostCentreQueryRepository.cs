using System.Data;
using Contracts.Interfaces.Lookups.Users;
using Dapper;
using FinanceManagement.Application.Common.Interfaces.ICostCentre;
using FinanceManagement.Application.CostCentre.Dto;

namespace FinanceManagement.Infrastructure.Repositories.CostCentre
{
    public class CostCentreQueryRepository : ICostCentreQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IUnitLookup _unitLookup;
        private readonly IDepartmentGroupLookup _departmentGroupLookup;
        private readonly IDepartmentLookup _departmentLookup;
        private readonly IUserLookup _userLookup;

        public CostCentreQueryRepository(
            IDbConnection dbConnection,
            IUnitLookup unitLookup,
            IDepartmentGroupLookup departmentGroupLookup,
            IDepartmentLookup departmentLookup,
            IUserLookup userLookup)
        {
            _dbConnection = dbConnection;
            _unitLookup = unitLookup;
            _departmentGroupLookup = departmentGroupLookup;
            _departmentLookup = departmentLookup;
            _userLookup = userLookup;
        }

        // Same-module joins: MiscMaster (level name) + self-join (parent name). Cross-module
        // names (Unit/DepartmentGroup/Department/Manager) are enriched via lookups below.
        private const string BaseSelect = @"
            cc.Id, cc.UnitId, cc.CompanyId,
            cc.CostCentreCode, cc.CostCentreName,
            cc.CentreLevelId, ml.Description AS CentreLevelName,
            cc.ParentCostCentreId, parent.CostCentreName AS ParentCostCentreName,
            cc.DepartmentGroupId, cc.DepartmentId,
            cc.ResponsibleManagerId, cc.EffectiveFromDate, cc.EffectiveToDate,
            cc.IsActive, cc.IsDeleted,
            cc.CreatedBy, cc.CreatedDate, cc.CreatedByName, cc.CreatedIP,
            cc.ModifiedBy, cc.ModifiedDate, cc.ModifiedByName, cc.ModifiedIP
        ";

        public async Task<(List<CostCentreDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm, int unitId)
        {
            var whereClause = "cc.IsDeleted = 0 AND cc.UnitId = @UnitId";
            if (!string.IsNullOrWhiteSpace(searchTerm))
                whereClause += " AND (cc.CostCentreCode LIKE @Search OR cc.CostCentreName LIKE @Search)";

            var query = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Finance.CostCentre cc
                WHERE {whereClause};

                SELECT {BaseSelect}
                FROM Finance.CostCentre cc
                LEFT JOIN Finance.MiscMaster ml ON cc.CentreLevelId = ml.Id
                LEFT JOIN Finance.CostCentre parent ON cc.ParentCostCentreId = parent.Id
                WHERE {whereClause}
                ORDER BY cc.CentreLevelId ASC, cc.CostCentreCode ASC, cc.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;
            ";

            var parameters = new
            {
                Search = $"%{searchTerm}%",
                UnitId = unitId,
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            var result = await _dbConnection.QueryMultipleAsync(query, parameters);
            var list = (await result.ReadAsync<CostCentreDto>()).ToList();
            var totalCount = await result.ReadFirstAsync<int>();

            await EnrichLookupNamesAsync(list);
            return (list, totalCount);
        }

        public async Task<CostCentreDto?> GetByIdAsync(int id)
        {
            var sql = $@"
                SELECT {BaseSelect}
                FROM Finance.CostCentre cc
                LEFT JOIN Finance.MiscMaster ml ON cc.CentreLevelId = ml.Id
                LEFT JOIN Finance.CostCentre parent ON cc.ParentCostCentreId = parent.Id
                WHERE cc.Id = @Id AND cc.IsDeleted = 0";

            var dto = await _dbConnection.QueryFirstOrDefaultAsync<CostCentreDto>(sql, new { Id = id });

            if (dto != null)
                await EnrichLookupNamesAsync(new List<CostCentreDto> { dto });

            return dto;
        }

        public async Task<IReadOnlyList<CostCentreLookupDto>> AutocompleteAsync(string term, int unitId, int? centreLevelId, CancellationToken ct)
        {
            var whereClause = "cc.IsDeleted = 0 AND cc.IsActive = 1 AND cc.UnitId = @UnitId";
            if (centreLevelId.HasValue && centreLevelId.Value > 0)
                whereClause += " AND cc.CentreLevelId = @CentreLevelId";
            if (!string.IsNullOrWhiteSpace(term))
                whereClause += " AND (cc.CostCentreCode LIKE @Term OR cc.CostCentreName LIKE @Term)";

            var sql = $@"
                SELECT cc.Id, cc.CostCentreCode, cc.CostCentreName, cc.CentreLevelId
                FROM Finance.CostCentre cc
                WHERE {whereClause}
                ORDER BY cc.CostCentreCode ASC";

            var result = await _dbConnection.QueryAsync<CostCentreLookupDto>(
                new CommandDefinition(sql, new { Term = $"%{term}%", UnitId = unitId, CentreLevelId = centreLevelId }, cancellationToken: ct));
            return result.ToList();
        }

        public async Task<bool> AlreadyExistsByCodeAsync(string costCentreCode, int unitId, int? id = null)
        {
            var sql = @"
                SELECT COUNT(1)
                FROM Finance.CostCentre
                WHERE CostCentreCode = @Code AND UnitId = @UnitId AND IsDeleted = 0";

            if (id.HasValue && id.Value > 0)
                sql += " AND Id != @Id";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Code = costCentreCode.Trim(), UnitId = unitId, Id = id });
            return count > 0;
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Finance.CostCentre
                WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<int> GetCentreLevelSortOrderAsync(int centreLevelId)
        {
            // The level row must belong to the COSTCENTRELEVEL misc type; SortOrder is the stable ordinal (1/2/3).
            const string sql = @"
                SELECT mm.SortOrder
                FROM Finance.MiscMaster mm
                INNER JOIN Finance.MiscTypeMaster mt ON mm.MiscTypeId = mt.Id
                WHERE mm.Id = @Id AND mm.IsDeleted = 0 AND mm.IsActive = 1
                  AND mt.MiscTypeCode = 'COSTCENTRELEVEL'";

            var sort = await _dbConnection.ExecuteScalarAsync<int?>(sql, new { Id = centreLevelId });
            return sort ?? 0;
        }

        public async Task<bool> ParentValidForLevelAsync(int? parentCostCentreId, int centreLevelId, int unitId)
        {
            var childSort = await GetCentreLevelSortOrderAsync(centreLevelId);
            if (childSort <= 0)
                return false;                                   // invalid level — handled by its own rule

            if (childSort == 1)
                return !parentCostCentreId.HasValue || parentCostCentreId.Value == 0;  // L1 must have no parent

            if (!parentCostCentreId.HasValue || parentCostCentreId.Value <= 0)
                return false;                                   // L2/L3 require a parent

            // Parent must exist in the same unit and sit exactly one level above the child.
            const string sql = @"
                SELECT ml.SortOrder
                FROM Finance.CostCentre cc
                INNER JOIN Finance.MiscMaster ml ON cc.CentreLevelId = ml.Id
                WHERE cc.Id = @ParentId AND cc.UnitId = @UnitId AND cc.IsDeleted = 0";

            var parentSort = await _dbConnection.ExecuteScalarAsync<int?>(sql, new { ParentId = parentCostCentreId.Value, UnitId = unitId });
            return parentSort.HasValue && parentSort.Value == childSort - 1;
        }

        public async Task<bool> PlantExistsForUnitAsync(int unitId)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1
                    FROM Finance.CostCentre cc
                    INNER JOIN Finance.MiscMaster ml ON cc.CentreLevelId = ml.Id
                    WHERE cc.UnitId = @UnitId AND cc.IsDeleted = 0 AND ml.SortOrder = 1
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { UnitId = unitId });
        }

        public async Task<bool> SoftDeleteValidationAsync(int id)
        {
            // Block deleting a node that still has children. Open transactions are added when the
            // journal engine lands (HasOpenTransactionsAsync stub below).
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Finance.CostCentre
                    WHERE ParentCostCentreId = @Id AND IsDeleted = 0
                ) THEN 1 ELSE 0 END";

            var hasChildren = await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = id });
            if (hasChildren)
                return true;

            return await HasOpenTransactionsAsync(id);
        }

        // STUB — returns false until the journal engine tags transactions to cost centres (AC#3 / Sprint 2).
        // The behaviour and message are already wired; only this body changes when real data exists.
        public Task<bool> HasOpenTransactionsAsync(int costCentreId) => Task.FromResult(false);

        private async Task EnrichLookupNamesAsync(List<CostCentreDto> list)
        {
            if (list.Count == 0)
                return;

            // Unit (Plant) — all rows share the same JWT unit, but resolve per distinct id to be safe.
            foreach (var unitId in list.Select(x => x.UnitId).Distinct())
            {
                var unit = await _unitLookup.GetByIdAsync(unitId);
                if (unit != null)
                    foreach (var item in list.Where(x => x.UnitId == unitId))
                        item.UnitName = unit.UnitName;
            }

            // Department Groups (L2/L3)
            var groupIds = list.Where(x => x.DepartmentGroupId.HasValue).Select(x => x.DepartmentGroupId!.Value).Distinct();
            foreach (var groupId in groupIds)
            {
                var group = await _departmentGroupLookup.GetByIdAsync(groupId);
                if (group != null)
                    foreach (var item in list.Where(x => x.DepartmentGroupId == groupId))
                        item.DepartmentGroupName = group.DepartmentGroupName;
            }

            // Departments (L3)
            var deptIds = list.Where(x => x.DepartmentId.HasValue).Select(x => x.DepartmentId!.Value).Distinct().ToList();
            if (deptIds.Count > 0)
            {
                var departments = await _departmentLookup.GetByIdsAsync(deptIds);
                var deptDict = departments.ToDictionary(d => d.DepartmentId, d => d.DepartmentName);
                foreach (var item in list.Where(x => x.DepartmentId.HasValue))
                    item.DepartmentName = deptDict.TryGetValue(item.DepartmentId!.Value, out var name) ? name : null;
            }

            // Responsible Managers (reserved — null for now, but enrich if present)
            var managerIds = list.Where(x => x.ResponsibleManagerId.HasValue).Select(x => x.ResponsibleManagerId!.Value).Distinct().ToList();
            if (managerIds.Count > 0)
            {
                var users = await _userLookup.GetByIdsAsync(managerIds);
                var userDict = users.ToDictionary(u => u.UserId, u => $"{u.FirstName} {u.LastName}".Trim());
                foreach (var item in list.Where(x => x.ResponsibleManagerId.HasValue))
                    item.ResponsibleManagerName = userDict.TryGetValue(item.ResponsibleManagerId!.Value, out var name) ? name : null;
            }
        }
    }
}
