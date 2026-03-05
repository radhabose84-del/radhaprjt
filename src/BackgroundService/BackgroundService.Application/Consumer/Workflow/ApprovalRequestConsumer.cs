using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using BackgroundService.Application.Interfaces.IInbox;
using BackgroundService.Application.Interfaces.IMiscMaster;
using BackgroundService.Application.Workflow.Common.Interfaces.IApprovalRequest;
using BackgroundService.Application.Workflow.Common.Interfaces.IWorkflowType;
using BackgroundService.Domain.Common;
using BackgroundService.Domain.Entities.Workflow;
using Contracts.Commands.Workflow;
using MassTransit;
using Microsoft.Extensions.Logging;


namespace BackgroundService.Application.Consumer.Workflow
{
    public class ApprovalRequestConsumer : IConsumer<CreateApprovalRequestCommand>
    {
        private readonly IWorkflowTypeQuery _workflowTypeQuery;
        private readonly IApprovalRequestCommand _approvalRequestCommand;
        private readonly IApprovalRequestQuery _approvalRequestQuery;
        private readonly IMiscMasterQueryRepository _miscMasterQuery;
        private readonly ILogger<ApprovalRequestConsumer> _logger;
        private readonly IInboxRepository _inbox;

        public ApprovalRequestConsumer(IWorkflowTypeQuery workflowTypeQuery, IApprovalRequestCommand approvalRequestCommand,
        IApprovalRequestQuery approvalRequestQuery, IMiscMasterQueryRepository miscMasterQuery, ILogger<ApprovalRequestConsumer> logger,
        IInboxRepository inbox)
        {
            _workflowTypeQuery = workflowTypeQuery;
            _approvalRequestCommand = approvalRequestCommand;
            _approvalRequestQuery = approvalRequestQuery;
            _miscMasterQuery = miscMasterQuery;
            _logger = logger;
            _inbox = inbox;
        }

        public async Task Consume(ConsumeContext<CreateApprovalRequestCommand> context)
        {
            var messageId = context.MessageId ?? Guid.NewGuid();
            const string consumerName = nameof(ApprovalRequestConsumer);

            if (await _inbox.IsAlreadyProcessedAsync(consumerName, messageId, context.CancellationToken))
            {
                _logger.LogInformation(
                    "Inbox dedup: duplicate skipped. Consumer={Consumer}, MessageId={MessageId}",
                    consumerName, messageId);
                return;
            }

            _logger.LogInformation("Transaction Created Request Consumer. ModuleTypeName : {ModuleTypeName},Request : {@Request}", context.Message.ModuleTypeName, context.Message);

            try
            {
                await _approvalRequestCommand.CreateBulkAsync(context.Message.ModuleTypeName, context.Message.ModuleTransactionId, context.Message.Payload);
                await _inbox.MarkAsProcessedAsync(consumerName, messageId, context.Message.CorrelationId, context.CancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Transaction Created Request Consumer. ModuleTypeName : {ModuleTypeName},Request : {@Request}", context.Message.ModuleTypeName, context.Message);
            }
        }
    }
}