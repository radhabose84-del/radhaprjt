using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Interfaces;
using BackgroundService.Application.Notification.Common.Interfaces;
using BackgroundService.Application.Workflow.Common.Interfaces.IApprovalRequest;
using BackgroundService.Domain.Common;
using BackgroundService.Domain.Entities.Workflow;
using BackgroundService.Infrastructure.Data.Notification;
using Contracts.Events.Workflow;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BackgroundService.Infrastructure.Repositories.Workflow.ApprovalRequests
{
    public class ApprovalRequestCommandRepository : IApprovalRequestCommand
    {
        private readonly NotificationDbContext _notificationDbContext;
        private readonly IDbConnection _dbConnection;
        private readonly IIPAddressService _ipAddressService;
        private readonly IEventPublisher _eventPublisher;
        private readonly ILookupRepository _lookupRepository;

        public ApprovalRequestCommandRepository(
            NotificationDbContext notificationDbContext,
            [FromKeyedServices("Notification")] IDbConnection dbConnection,
            IIPAddressService ipAddressService,
            IEventPublisher eventPublisher,
            ILookupRepository lookupRepository)
        {
            _notificationDbContext = notificationDbContext;
            _dbConnection = dbConnection;
            _ipAddressService = ipAddressService;
            _eventPublisher = eventPublisher;
            _lookupRepository = lookupRepository;
        }

        public async Task<int> Approve(ApprovalRequest approvalRequest,string ApprovalRequestLines,CancellationToken ct)
        {
                var p = new DynamicParameters();
        p.Add("@HeaderId", approvalRequest.Id, DbType.Int32);
        p.Add("@JsonUpdates", ApprovalRequestLines, DbType.String);
        p.Add("@Approved", MiscEnumEntity.Approved, DbType.String);
        p.Add("@Rejected", MiscEnumEntity.Rejected, DbType.String);
        p.Add("@Pending", MiscEnumEntity.Pending, DbType.String);
        p.Add("@ModifiedBy", _ipAddressService.GetUserId(), DbType.Int32);
        p.Add("@NewHeaderStatusId", dbType: DbType.Int32, direction: ParameterDirection.Output);

        // Use CommandDefinition to pass the cancellation token
        var cmd = new CommandDefinition(
            commandText: "[AppData].[usp_Approval_UpdateLines]",
            parameters: p,
            commandType: CommandType.StoredProcedure,
            cancellationToken: ct
        );

         await _dbConnection.ExecuteAsync(cmd);

          return p.Get<int>("@NewHeaderStatusId");
        }

        public async Task<bool> Approve(ApprovalRequest approvalRequest, CancellationToken ct)
        {
            var approvalReq = await _notificationDbContext.ApprovalRequest.FirstOrDefaultAsync(u => u.Id == approvalRequest.Id);
            if (approvalReq != null)
            {
                approvalReq.StatusId = approvalRequest.StatusId;
                approvalReq.Remark = approvalRequest.Remark;
                approvalReq.ModifiedBy = approvalRequest.ModifiedBy;
                approvalReq.ModifiedByName = approvalRequest.ModifiedByName;
                approvalReq.ModifiedDate = approvalRequest.ModifiedDate;
                approvalReq.ModifiedIP = approvalRequest.ModifiedIP;
                return await _notificationDbContext.SaveChangesAsync() > 0;
            }
            return false;
        }

        public async Task RevertStatusAsync(int approvalRequestHeaderId, int statusId, CancellationToken ct)
        {
            var entity = await _notificationDbContext.ApprovalRequest
                .FirstOrDefaultAsync(u => u.Id == approvalRequestHeaderId, ct);
            if (entity != null)
            {
                entity.StatusId = statusId;
                entity.ModifiedBy = _ipAddressService.GetUserId();
                entity.ModifiedByName = _ipAddressService.GetUserName();
                entity.ModifiedIP = _ipAddressService.GetSystemIPAddress();
                await _notificationDbContext.SaveChangesAsync(ct);
            }
        }

        public async Task<bool> CreateBulkAsync(string workflowType, int transactionId, string contextJson, int? transactionTypeId = null)
        {
            try
            {
                var menuId = await _lookupRepository.GetMenuIdByNameAsync(workflowType);

                // Fallback: when multiple PO types share one menu (e.g. "Purchase Order"),
                // the handler may pass a specific ModuleTypeName like "Contract Purchase Order"
                // that has no matching MenuName. Resolve MenuId from TransactionTypeMaster instead.
                if (menuId == null && transactionTypeId.HasValue)
                {
                    menuId = await _lookupRepository.GetMenuIdByTransactionTypeIdAsync(
                        transactionTypeId.Value);
                }

                var procParams = new
                {
                    MenuId = menuId,
                    WorkflowCode = workflowType,
                    TransactionId = transactionId,
                    ContextJson = contextJson,
                    TransactionTypeId = transactionTypeId
                };

                await _dbConnection.ExecuteAsync(
                    "[AppData].[sp_EvaluateApproval]",
                    procParams,
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: 60);

                return true;
            }
            catch (Exception ex)
            {
                var correlationId = Guid.NewGuid();
                var @event = new ApprovalRequestFailedEvent
                {
                    CorrelationId = correlationId,
                    ModuleTypeName = workflowType,
                    ModuleTransactionId = transactionId
                };

                await _eventPublisher.SaveEventAsync(@event);
                await _eventPublisher.PublishPendingEventsAsync();

                // Preserve the original exception (SQL error, null MenuId, timeout, etc.)
                // as the inner exception — otherwise the real failure cause is lost.
                throw new InvalidOperationException(
                    $"EvaluateApproval Stored Procedure Failed (WorkflowType='{workflowType}', " +
                    $"TransactionId={transactionId}, TransactionTypeId={transactionTypeId}): {ex.Message}",
                    ex);
            }
        }

    }
}