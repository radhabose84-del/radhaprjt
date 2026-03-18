using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Dtos.Workflow;
using Contracts.Interfaces.Lookups.Workflow;
using Dapper;
namespace BackgroundService.Infrastructure.Repositories.Lookups.Workflow
{
    internal sealed class WorkflowLookupRepository : IWorkflowLookup
    {
        private readonly IDbConnection _dbConnection;

        public WorkflowLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<List<ApprovalRequestStatusDto>> GetAllApprovalRequestStatusAsync(string moduleTypeName)
        {
            const string sql = @"
                SELECT
                    ar.Id,
                    ar.Id AS ApprovalRequestId,
                    ar.ModuleTransactionId,
                    mm.Description AS CurrentStatus,
                    ar.WorkflowType AS ModuleTypeName
                FROM AppData.ApprovalRequest ar
                INNER JOIN AppData.MiscMaster mm ON ar.StatusId = mm.Id
                WHERE ar.WorkflowType = @ModuleTypeName
                  AND ar.IsDeleted = 0;
            ";

            var result = await _dbConnection.QueryAsync<ApprovalRequestStatusDto>(sql, new { ModuleTypeName = moduleTypeName });
            return result.ToList();
        }

        public async Task<List<ApprovalRequestLineStatusDto>> GetApprovalRequestLineStatusAsync(
            string moduleTypeName,
            IEnumerable<int> moduleTransactionIds,
            int userId)
        {
            var ids = moduleTransactionIds?.Distinct().ToArray() ?? System.Array.Empty<int>();
            if (ids.Length == 0)
                return new List<ApprovalRequestLineStatusDto>();

            const string sql = @"
                SELECT
                    arl.Id AS ApprovalRequestLineTransactionId,
                    arl.ModuleLineTransactionId,
                    mm.Description AS Status
                FROM AppData.ApprovalRequestLine arl
                INNER JOIN AppData.ApprovalRequest ar ON arl.ApprovalRequestId = ar.Id
                INNER JOIN AppData.MiscMaster mm ON arl.StatusId = mm.Id
                WHERE ar.WorkflowType = @ModuleTypeName
                  AND ar.ModuleTransactionId IN @Ids
                  AND ar.IsDeleted = 0
                  AND ar.ApproverValue = CAST(@UserId AS NVARCHAR(50));
            ";

            var result = await _dbConnection.QueryAsync<ApprovalRequestLineStatusDto>(
                sql, new { ModuleTypeName = moduleTypeName, Ids = ids, UserId = userId });
            return result.ToList();
        }

        public async Task<List<ApproverListDto>> GetApproverListAsync(string moduleTypeName, IEnumerable<int> moduleTransactionIds)
        {
            var ids = moduleTransactionIds?.Distinct().ToArray() ?? System.Array.Empty<int>();
            if (ids.Length == 0)
                return new List<ApproverListDto>();

            const string sql = @"
                SELECT
                    arl.Id AS ApprovalRequestLineId,
                    ar.ModuleTransactionId,
                    mm.Description AS Status,
                    ar.ApproverBinding,
                    ar.ApproverValue,
                    ar.Id AS ApprovalRequestId,
                    ISNULL(asd.IsEdit, 0) AS IsEdit
                FROM AppData.ApprovalRequestLine arl
                INNER JOIN AppData.ApprovalRequest ar ON arl.ApprovalRequestId = ar.Id
                INNER JOIN AppData.MiscMaster mm ON arl.StatusId = mm.Id
                INNER JOIN AppData.ApprovalStepDetail asd ON ar.ApprovalStepDetailId = asd.Id
                WHERE ar.WorkflowType = @ModuleTypeName
                  AND ar.ModuleTransactionId IN @Ids
                  AND ar.IsDeleted = 0
                  AND mm.Description = 'Pending';
            ";

            var result = await _dbConnection.QueryAsync<ApproverListDto>(
                sql, new { ModuleTypeName = moduleTypeName, Ids = ids });
            return result.ToList();
        }

        public async Task<List<ApprovalRequestLineStatusDto>> GetApprovalRequestLineAsync(
            string moduleTypeName,
            IEnumerable<int> moduleTransactionIds,
            int userId)
        {
            return await GetApprovalRequestLineStatusAsync(moduleTypeName, moduleTransactionIds, userId);
        }

        public async Task<bool> IsApproveWorkflowConfigureAsync(string menuName, int unitId, int departmentId)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1
                    FROM AppData.ApprovalStepDetail asd
                    INNER JOIN AppData.WorkflowType wt ON asd.WorkFlowTypeId = wt.Id
                    INNER JOIN AppData.Menus m ON wt.MenuId = m.Id
                    INNER JOIN AppData.ApprovalStepUnitMapping asum ON asd.Id = asum.ApprovalStepDetailId
                    WHERE m.MenuName = @MenuName
                      AND asum.UnitId = @UnitId
                      AND asd.IsDeleted = 0
                      AND asd.IsActive = 1
                ) THEN 1 ELSE 0 END;
            ";

            var result = await _dbConnection.ExecuteScalarAsync<int>(
                sql, new { MenuName = menuName, UnitId = unitId });
            return result == 1;
        }
    }
}
