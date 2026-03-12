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
                return await _notificationDbContext.SaveChangesAsync() > 0;
            }
            return false;
        }

        public async Task<bool> CreateBulkAsync(string workflowType, int transactionId, string contextJson)
        {
            try
            {
                var menuId = await _lookupRepository.GetMenuIdByNameAsync(workflowType);
                var procParams = new
                {
                    MenuId = menuId,
                    WorkflowCode = workflowType,
                    TransactionId = transactionId,
                    ContextJson = contextJson
                };

                await _dbConnection.ExecuteAsync(
                    "[AppData].[sp_EvaluateApproval]",
                    procParams,
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: 60);

                return true;
            }
            catch (Exception)
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

                throw new Exception("EvaluateApproval Stored Procedure Failed");
            }
        }

    }
}