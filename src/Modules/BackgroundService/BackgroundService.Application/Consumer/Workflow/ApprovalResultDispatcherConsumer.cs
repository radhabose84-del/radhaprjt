using Contracts.Commands.Budget;
using Contracts.Commands.Inventory;
using Contracts.Commands.Party;
using Contracts.Commands.Project;
using Contracts.Commands.Purchase;
using Contracts.Commands.Sales;
using Contracts.Events.Workflow;
using BackgroundService.Application.Interfaces.IInbox;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace BackgroundService.Application.Consumer.Workflow;

/// <summary>
/// Receives ApprovedRejectedEvent (published by ApproveApprovalRequestCommandHandler after
/// sp_EvaluateApproval / usp_Approval_UpdateLines) and routes it to the module-specific
/// update command queue so each module can update its own transaction status.
///
/// Routing table:
///   Purchase types   → UpdateApprovedRejectedPurchaseCommand   → approved-rejected-purchase-task-queue
///   Budget types     → UpdateApprovedRejectedBudgetCommand      → approved-rejected-budget-task-queue
///   Inventory types  → UpdateApprovedRejectedInventoryCommand   → approved-rejected-inventory-task-queue
///   MaterialRequest  → both Purchase and Inventory queues (dual-module update)
///   Party types      → UpdateApprovedRejectedPartyCommand    → approved-rejected-party-task-queue
/// </summary>
public class ApprovalResultDispatcherConsumer : IConsumer<ApprovedRejectedEvent>
{
    private static readonly HashSet<string> PurchaseTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "PurchaseIndent", "QuotationComparison", "Item Price Master", "POLocal",
        "MaterialRequest", "ServicePO", "ServiceEntrySheet", "IssueReturn"
    };

    private static readonly HashSet<string> BudgetTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "BudgetRequest"
    };

    private static readonly HashSet<string> InventoryTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Material Requisition Slip"
    };

    private static readonly HashSet<string> PartyTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Party"
    };

    private static readonly HashSet<string> ProjectTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "ProjectMaster", "Project Master"
    };

    private static readonly HashSet<string> SalesTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Invoice",
        "Sales Order",
        "STO",
        "Delivery Challan",
        "Complaints",
        "Sales Order Amendment"
    };    private readonly IInboxRepository _inbox;
    private readonly ILogger<ApprovalResultDispatcherConsumer> _logger;

    public ApprovalResultDispatcherConsumer(IInboxRepository inbox, ILogger<ApprovalResultDispatcherConsumer> logger)
    {
        _inbox = inbox;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ApprovedRejectedEvent> context)
    {
        var messageId = context.MessageId ?? Guid.NewGuid();
        const string consumerName = nameof(ApprovalResultDispatcherConsumer);

        if (await _inbox.IsAlreadyProcessedAsync(consumerName, messageId, context.CancellationToken))
        {
            _logger.LogInformation("Inbox dedup: skipped. Consumer={C}, MessageId={M}", consumerName, messageId);
            return;
        }

        var msg = context.Message;

        _logger.LogInformation(
            "ApprovalResultDispatcher: ModuleTypeName={Module}, Status={Status}, TransactionId={Id}",
            msg.ModuleTypeName, msg.Status, msg.ModuleTransactionId);

        if (PurchaseTypes.Contains(msg.ModuleTypeName))
        {
            await context.Publish(new UpdateApprovedRejectedPurchaseCommand
            {
                CorrelationId = msg.CorrelationId,
                ModuleTransactionId = msg.ModuleTransactionId,
                ModuleTypeName = msg.ModuleTypeName,
                Status = msg.Status,
                LineStatus = msg.LineStatus,
                PartyContacts = msg.PartyContacts,
                DynamicFields = msg.DynamicFields,
                ModifiedBy = msg.ModifiedBy,
                ModifiedByName = msg.ModifiedByName,
                ModifiedIP = msg.ModifiedIP
            });
        }

        if (BudgetTypes.Contains(msg.ModuleTypeName))
        {
            await context.Publish(new UpdateApprovedRejectedBudgetCommand
            {
                CorrelationId = msg.CorrelationId,
                ModuleTransactionId = msg.ModuleTransactionId,
                ModuleTypeName = msg.ModuleTypeName,
                Status = msg.Status,
                ModifiedBy = msg.ModifiedBy,
                ModifiedByName = msg.ModifiedByName,
                ModifiedIP = msg.ModifiedIP
            });
        }

        if (InventoryTypes.Contains(msg.ModuleTypeName))
        {
            await context.Publish(new UpdateApprovedRejectedInventoryCommand
            {
                CorrelationId = msg.CorrelationId,
                ModuleTransactionId = msg.ModuleTransactionId,
                ModuleTypeName = msg.ModuleTypeName,
                Status = msg.Status,
                LineStatus = msg.LineStatus,
                PartyContacts = msg.PartyContacts,
                DynamicFields = msg.DynamicFields,
                ModifiedBy = msg.ModifiedBy,
                ModifiedByName = msg.ModifiedByName,
                ModifiedIP = msg.ModifiedIP
            });
        }

        if (PartyTypes.Contains(msg.ModuleTypeName))
        {
            await context.Publish(new UpdateApprovedRejectedPartyCommand
            {
                CorrelationId = msg.CorrelationId,
                ModuleTransactionId = msg.ModuleTransactionId,
                ModuleTypeName = msg.ModuleTypeName,
                Status = msg.Status,
                LineStatus = msg.LineStatus,
                PartyContacts = msg.PartyContacts,
                ModifiedBy = msg.ModifiedBy,
                ModifiedByName = msg.ModifiedByName,
                ModifiedIP = msg.ModifiedIP
            });
        }

        if (ProjectTypes.Contains(msg.ModuleTypeName))
        {
            await context.Publish(new UpdateApprovedRejectedProjectCommand
            {
                CorrelationId = msg.CorrelationId,
                ModuleTransactionId = msg.ModuleTransactionId,
                ModuleTypeName = msg.ModuleTypeName,
                Status = msg.Status
            });
        }
        if (SalesTypes.Contains(msg.ModuleTypeName))
        {
            await context.Publish(new UpdateApprovedRejectedSalesCommand
            {
                CorrelationId = msg.CorrelationId,
                ModuleTransactionId = msg.ModuleTransactionId,
                ModuleTypeName = msg.ModuleTypeName,
                Status = msg.Status,
                LineStatus = msg.LineStatus,
                ModifiedBy = msg.ModifiedBy,
                ModifiedByName = msg.ModifiedByName,
                ModifiedIP = msg.ModifiedIP
            });
        }
        await _inbox.MarkAsProcessedAsync(consumerName, messageId, msg.CorrelationId, context.CancellationToken);
    }
}
