using System.Data;
using Contracts.Dtos.Workflow;
using Contracts.Interfaces.Workflow;
using Dapper;
using Microsoft.Extensions.DependencyInjection;

namespace BackgroundService.Infrastructure.Repositories.Workflow.ApprovalRequests
{
    internal sealed class ApprovalRequestRefProvider : IApprovalRequestRefProvider
    {
        private readonly IDbConnection _dbConnection;

        public ApprovalRequestRefProvider([FromKeyedServices("Notification")] IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<IReadOnlyList<ApprovalRequestRefDto>> GetByModuleAsync(
            string workflowType, IEnumerable<int> moduleTransactionIds, CancellationToken ct = default)
        {
            var ids = moduleTransactionIds?.Distinct().ToList() ?? new List<int>();
            if (ids.Count == 0)
                return new List<ApprovalRequestRefDto>();

            const string sql = @"
                SELECT Id AS ApprovalRequestId, ModuleTransactionId, StatusId, ApproverValue
                FROM AppData.ApprovalRequest
                WHERE WorkflowType = @WorkflowType AND ModuleTransactionId IN @Ids;";

            var rows = await _dbConnection.QueryAsync<ApprovalRequestRefDto>(
                new CommandDefinition(sql, new { WorkflowType = workflowType, Ids = ids }, cancellationToken: ct));
            return rows.ToList();
        }
    }
}
