using System.Text.Json;
using BackgroundService.Application.Interfaces.IInbox;
using BackgroundService.Application.Interfaces.IMiscMaster;
using BackgroundService.Application.Notification.Common.Interfaces;
using BackgroundService.Application.Workflow.Common.Interfaces.IApprovalRequest;
using BackgroundService.Application.Workflow.Common.Interfaces.IWorkflowType;
using BackgroundService.Domain.Common;
using Contracts.Commands.Workflow;
using Contracts.Dtos.Common;
using Contracts.Dtos.Purchase;
using Contracts.Events.Workflow;
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
        private readonly IEventPublisher _eventPublisher;
        private readonly ILookupRepository _lookupRepository;

        public ApprovalRequestConsumer(
            IWorkflowTypeQuery workflowTypeQuery,
            IApprovalRequestCommand approvalRequestCommand,
            IApprovalRequestQuery approvalRequestQuery,
            IMiscMasterQueryRepository miscMasterQuery,
            ILogger<ApprovalRequestConsumer> logger,
            IInboxRepository inbox,
            IEventPublisher eventPublisher,
            ILookupRepository lookupRepository)
        {
            _workflowTypeQuery = workflowTypeQuery;
            _approvalRequestCommand = approvalRequestCommand;
            _approvalRequestQuery = approvalRequestQuery;
            _miscMasterQuery = miscMasterQuery;
            _logger = logger;
            _inbox = inbox;
            _eventPublisher = eventPublisher;
            _lookupRepository = lookupRepository;
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

            _logger.LogInformation(
                "Transaction Created Request Consumer. ModuleTypeName : {ModuleTypeName}, Request : {@Request}",
                context.Message.ModuleTypeName, context.Message);

            try
            {
                await _approvalRequestCommand.CreateBulkAsync(
                    context.Message.ModuleTypeName,
                    context.Message.ModuleTransactionId,
                    context.Message.Payload,
                    context.Message.TransactionTypeId);

                // ── Resolve the WorkflowType to match against ApprovalRequest ─────
                // ApprovalRequest rows are keyed by the transaction type name when a
                // TransactionTypeId is supplied (PO sub-types, OCR, etc.); fall back to
                // ModuleTypeName when no TransactionTypeId is present.
                var workflowType = context.Message.ModuleTypeName;
                if (context.Message.TransactionTypeId.HasValue && context.Message.TransactionTypeId.Value > 0)
                {
                    var typeNames = await _lookupRepository.GetTransactionTypeNamesAsync(
                        new[] { context.Message.TransactionTypeId.Value },
                        context.CancellationToken);

                    if (typeNames.TryGetValue(context.Message.TransactionTypeId.Value, out var typeName)
                        && !string.IsNullOrWhiteSpace(typeName))
                    {
                        workflowType = typeName;
                    }
                }

                // ── Auto-approve if no approval rules were found ──────────────────
                // sp_EvaluateApproval may return without creating any ApprovalRequest
                // rows when no matching approval rules are configured (e.g. EmergencyPO).
                // In that case, publish an ApprovedRejectedEvent so the module consumer
                // updates the transaction status to Approved automatically.
                var hasApproval = await _approvalRequestQuery.HasApprovalRequestAsync(
                    context.Message.ModuleTransactionId,
                    workflowType,
                    context.CancellationToken);

                if (!hasApproval)
                {
                    _logger.LogInformation(
                        "No approval rules found for {ModuleTypeName} TransactionId={TransactionId}. Auto-approving.",
                        context.Message.ModuleTypeName,
                        context.Message.ModuleTransactionId);

                    var autoApproveEvent = new ApprovedRejectedEvent
                    {
                        CorrelationId       = context.Message.CorrelationId,
                        ModuleTransactionId = context.Message.ModuleTransactionId,
                        ModuleTypeName      = context.Message.ModuleTypeName,
                        Status              = MiscEnumEntity.Approved,
                        TransactionTypeId   = context.Message.TransactionTypeId,
                        LineStatus          = new List<UpdateLineStatusDto>(),
                        PartyContacts       = new List<PartyRefDto>(),
                        DynamicFields       = new List<JsonElement>()
                    };

                    await _eventPublisher.SaveEventAsync(autoApproveEvent);
                    await _eventPublisher.PublishPendingEventsAsync();
                }

                // ── Mark as processed AFTER all business logic succeeds ───────────
                // Previously this was called right after CreateBulkAsync, before
                // the auto-approve check. If HasApprovalRequestAsync or the event
                // publisher threw, MassTransit would retry with the same MessageId,
                // but the inbox dedup would block the retry (already marked processed),
                // leaving the PO stuck in Pending with no auto-approve event published.
                await _inbox.MarkAsProcessedAsync(
                    consumerName, messageId,
                    context.Message.CorrelationId, context.CancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Transaction Created Request Consumer failed. ModuleTypeName : {ModuleTypeName}, TransactionId : {TransactionId}",
                    context.Message.ModuleTypeName, context.Message.ModuleTransactionId);
                throw; // Re-throw so MassTransit can retry or move to error queue
            }
        }
    }
}
