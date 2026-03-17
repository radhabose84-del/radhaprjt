// using System.Data;
// using Contracts.Dtos.Workflow;
// using Contracts.Interfaces.Lookups.Workflow;
// using Dapper;
// using Contracts.Interfaces;

// namespace PurchaseManagement.Infrastructure.Repositories.Lookups.Workflow
// {
//     internal sealed class WorkflowLookupRepository : IWorkflowLookup
//     {
//         private readonly IDbConnection _dbConnection;
//         private readonly IIPAddressService _ipAddressService;

//         public WorkflowLookupRepository(IDbConnection dbConnection, IIPAddressService ipAddressService)
//         {
//             _dbConnection = dbConnection;
//             _ipAddressService = ipAddressService;
//         }

//         public async Task<List<ApprovalRequestStatusDto>> GetAllApprovalRequestStatusAsync(string moduleTypeName)
//         {
//             if (!await EnsureWorkflowTablesExistAsync("Workflow.ApprovalRequest", "Workflow.ModuleType"))
//                 return new List<ApprovalRequestStatusDto>();

//             const string sql = @"
//                 SELECT
//                     ar.Id AS ApprovalRequestId,
//                     ar.ModuleTransactionId,
//                     ar.Status,
//                     mt.ModuleTypeName
//                 FROM Workflow.ApprovalRequest ar
//                 INNER JOIN Workflow.ModuleType mt ON ar.ModuleTypeId = mt.Id
//                 WHERE mt.ModuleTypeName = @ModuleTypeName
//                   AND ar.IsDeleted = 0;
//             ";

//             var result = await _dbConnection.QueryAsync<ApprovalRequestStatusDto>(sql, new { ModuleTypeName = moduleTypeName });
//             return result.ToList();
//         }

//         public async Task<List<ApprovalRequestLineStatusDto>> GetApprovalRequestLineStatusAsync(
//             string moduleTypeName,
//             IEnumerable<int> moduleTransactionIds,
//             int userId)
//         {
//             var ids = moduleTransactionIds?.Distinct().ToArray() ?? System.Array.Empty<int>();
//             if (ids.Length == 0)
//                 return new List<ApprovalRequestLineStatusDto>();
//             if (!await EnsureWorkflowTablesExistAsync("Workflow.ApprovalRequestLine", "Workflow.ApprovalRequest", "Workflow.ModuleType"))
//                 return new List<ApprovalRequestLineStatusDto>();

//             const string sql = @"
//                 SELECT
//                     arl.Id AS ApprovalRequestLineId,
//                     ar.ModuleTransactionId,
//                     arl.Status,
//                     arl.ApproverBinding,
//                     arl.ApproverValue,
//                     ar.Id AS ApprovalRequestId
//                 FROM Workflow.ApprovalRequestLine arl
//                 INNER JOIN Workflow.ApprovalRequest ar ON arl.ApprovalRequestId = ar.Id
//                 INNER JOIN Workflow.ModuleType mt ON ar.ModuleTypeId = mt.Id
//                 WHERE mt.ModuleTypeName = @ModuleTypeName
//                   AND ar.ModuleTransactionId IN @Ids
//                   AND ar.IsDeleted = 0
//                   AND arl.ApproverValue = CAST(@UserId AS NVARCHAR(50));
//             ";

//             var result = await _dbConnection.QueryAsync<ApprovalRequestLineStatusDto>(
//                 sql, new { ModuleTypeName = moduleTypeName, Ids = ids, UserId = userId });
//             return result.ToList();
//         }

//         public async Task<List<ApproverListDto>> GetApproverListAsync(string moduleTypeName, IEnumerable<int> moduleTransactionIds)
//         {
//             var ids = moduleTransactionIds?.Distinct().ToArray() ?? System.Array.Empty<int>();
//             if (ids.Length == 0)
//                 return new List<ApproverListDto>();
//             if (!await EnsureWorkflowTablesExistAsync("Workflow.ApprovalRequestLine", "Workflow.ApprovalRequest", "Workflow.ModuleType"))
//                 return new List<ApproverListDto>();

//             const string sql = @"
//                 SELECT
//                     arl.Id AS ApprovalRequestLineId,
//                     ar.ModuleTransactionId,
//                     arl.Status,
//                     arl.ApproverBinding,
//                     arl.ApproverValue,
//                     ar.Id AS ApprovalRequestId,
//                     ISNULL(ar.IsEdit, 0) AS IsEdit
//                 FROM Workflow.ApprovalRequestLine arl
//                 INNER JOIN Workflow.ApprovalRequest ar ON arl.ApprovalRequestId = ar.Id
//                 INNER JOIN Workflow.ModuleType mt ON ar.ModuleTypeId = mt.Id
//                 WHERE mt.ModuleTypeName = @ModuleTypeName
//                   AND ar.ModuleTransactionId IN @Ids
//                   AND ar.IsDeleted = 0
//                   AND arl.Status = 'Pending';
//             ";

//             var result = await _dbConnection.QueryAsync<ApproverListDto>(
//                 sql, new { ModuleTypeName = moduleTypeName, Ids = ids });
//             return result.ToList();
//         }

//         public Task<List<ApprovalRequestLineStatusDto>> GetApprovalRequestLineAsync(
//             string moduleTypeName,
//             IEnumerable<int> moduleTransactionIds,
//             int userId)
//         {
//             return GetApprovalRequestLineStatusAsync(moduleTypeName, moduleTransactionIds, userId);
//         }

//         public async Task<bool> IsApproveWorkflowConfigureAsync(string menuName, int unitId, int departmentId)
//         {
//             if (!await EnsureWorkflowTablesExistAsync("Workflow.WorkflowConfiguration", "Workflow.ModuleType"))
//                 return false;
//             const string sql = @"
//                 SELECT CASE WHEN EXISTS (
//                     SELECT 1
//                     FROM Workflow.WorkflowConfiguration wc
//                     INNER JOIN Workflow.ModuleType mt ON wc.ModuleTypeId = mt.Id
//                     WHERE mt.ModuleTypeName = @MenuName
//                       AND wc.UnitId = @UnitId
//                       AND wc.DepartmentId = @DepartmentId
//                       AND wc.IsDeleted = 0
//                       AND wc.IsActive = 1
//                 ) THEN 1 ELSE 0 END;
//             ";

//             var result = await _dbConnection.ExecuteScalarAsync<int>(
//                 sql, new { MenuName = menuName, UnitId = unitId, DepartmentId = departmentId });
//             return result == 1;
//         }

//         private async Task<bool> EnsureWorkflowTablesExistAsync(params string[] tableNames)
//         {
//             const string sql = "SELECT CASE WHEN OBJECT_ID(@TableName, 'U') IS NOT NULL THEN 1 ELSE 0 END";
//             foreach (var tableName in tableNames)
//             {
//                 var exists = await _dbConnection.ExecuteScalarAsync<int>(sql, new { TableName = tableName });
//                 if (exists == 0)
//                     return false;
//             }
//             return true;
//         }
//     }
// }
