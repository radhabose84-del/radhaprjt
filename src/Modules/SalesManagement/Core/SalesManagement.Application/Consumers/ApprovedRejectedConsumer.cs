using System.Text.Json;
using Contracts.Commands.Finance;
using Contracts.Commands.Sales;
using Contracts.Events.Notifications;
using Contracts.Events.Sales;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Common;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Sales;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Workflow;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using SalesManagement.Application.Common.Interfaces.IDeliveryChallan;
using SalesManagement.Application.Common.Interfaces.IInvoice;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Application.Common.Interfaces.ISalesOrder;
using SalesManagement.Application.Common.Interfaces.ISalesOrderAmendment;
using SalesManagement.Application.Common.Interfaces.ISalesQuotation;
using SalesManagement.Application.Common.Interfaces.ISalesQuotationAmendment;
using SalesManagement.Application.Common.Interfaces.IComplaint;
using SalesManagement.Application.Common.Interfaces.IComplaintQCReview;
using SalesManagement.Application.Common.Interfaces.IComplaintResolution;
using SalesManagement.Application.Common.Interfaces.IStoHeader;
using SalesManagement.Domain.Common;

namespace SalesManagement.Application.Consumers
{
    public class ApprovedRejectedConsumer : IConsumer<UpdateApprovedRejectedSalesCommand>
    {
        private readonly IInvoiceCommandRepository _invoiceCommandRepo;
        private readonly ISalesOrderCommandRepository _salesOrderCommandRepo;
        private readonly ISalesOrderAmendmentCommandRepository _amendmentCommandRepo;
        private readonly ISalesQuotationCommandRepository _sqCommandRepo;
        private readonly ISalesQuotationAmendmentCommandRepository _sqAmendmentCommandRepo;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
        private readonly IStoHeaderCommandRepository _stoHeaderCommandRepo;
        private readonly IDeliveryChallanCommandRepository _dcCommandRepo;
        private readonly IComplaintCommandRepository _complaintCommandRepo;
        private readonly IComplaintQueryRepository _complaintQueryRepo;
        private readonly IComplaintQCReviewQueryRepository _qcReviewQueryRepo;
        private readonly IComplaintResolutionQueryRepository _resolutionQueryRepo;
        private readonly IMediator _mediator;
        private readonly ILogger<ApprovedRejectedConsumer> _logger;
        private readonly IOfficerAgentUserLookup _officerAgentUserLookup;
        private readonly IAppDataMiscMasterLookup _appDataMiscLookup;
        private readonly IPartyDetailLookup _partyDetailLookup;
        private readonly IWorkflowLookup _workflowLookup;
        private readonly IUnitLookup _unitLookup;
        private readonly IDepartmentUserLookup _departmentUserLookup;

        public ApprovedRejectedConsumer(
            IInvoiceCommandRepository invoiceCommandRepo,
            ISalesOrderCommandRepository salesOrderCommandRepo,
            ISalesOrderAmendmentCommandRepository amendmentCommandRepo,
            ISalesQuotationCommandRepository sqCommandRepo,
            ISalesQuotationAmendmentCommandRepository sqAmendmentCommandRepo,
            IMiscMasterQueryRepository miscMasterQueryRepository,
            IStoHeaderCommandRepository stoHeaderCommandRepo,
            IDeliveryChallanCommandRepository dcCommandRepo,
            IComplaintCommandRepository complaintCommandRepo,
            IComplaintQueryRepository complaintQueryRepo,
            IComplaintQCReviewQueryRepository qcReviewQueryRepo,
            IComplaintResolutionQueryRepository resolutionQueryRepo,
            IMediator mediator,
            ILogger<ApprovedRejectedConsumer> logger,
            IOfficerAgentUserLookup officerAgentUserLookup,
            IAppDataMiscMasterLookup appDataMiscLookup,
            IPartyDetailLookup partyDetailLookup,
            IWorkflowLookup workflowLookup,
            IUnitLookup unitLookup,
            IDepartmentUserLookup departmentUserLookup)
        {
            _invoiceCommandRepo = invoiceCommandRepo;
            _salesOrderCommandRepo = salesOrderCommandRepo;
            _amendmentCommandRepo = amendmentCommandRepo;
            _sqCommandRepo = sqCommandRepo;
            _sqAmendmentCommandRepo = sqAmendmentCommandRepo;
            _miscMasterQueryRepository = miscMasterQueryRepository;
            _stoHeaderCommandRepo = stoHeaderCommandRepo;
            _dcCommandRepo = dcCommandRepo;
            _complaintCommandRepo = complaintCommandRepo;
            _complaintQueryRepo = complaintQueryRepo;
            _qcReviewQueryRepo = qcReviewQueryRepo;
            _resolutionQueryRepo = resolutionQueryRepo;
            _mediator = mediator;
            _logger = logger;
            _officerAgentUserLookup = officerAgentUserLookup;
            _appDataMiscLookup = appDataMiscLookup;
            _partyDetailLookup = partyDetailLookup;
            _workflowLookup = workflowLookup;
            _unitLookup = unitLookup;
            _departmentUserLookup = departmentUserLookup;
        }

        public async Task Consume(ConsumeContext<UpdateApprovedRejectedSalesCommand> context)
        {
            var msg = context.Message;

            _logger.LogInformation(
                "ApprovedRejectedSalesConsumer: ModuleTypeName={ModuleType}, Status={Status}, TransactionId={Id}",
                msg.ModuleTypeName, msg.Status, msg.ModuleTransactionId);

            try
            {
                // Route by ModuleTypeName
                switch (msg.ModuleTypeName)
                {
                    case MiscEnumEntity.TransactionTypeInvoice:
                        await HandleInvoiceApprovalAsync(msg, msg.DynamicFields, context.CancellationToken);
                        break;

                    case MiscEnumEntity.TransactionTypeSalesOrder:
                        await HandleSalesOrderApprovalAsync(context);
                        break;

                    case MiscEnumEntity.TransactionTypeSalesOrderAmendment:
                        await HandleSalesOrderAmendmentApprovalAsync(msg, context.CancellationToken);
                        break;

                    case MiscEnumEntity.TransactionTypeSalesQuotation:
                        await HandleSalesQuotationApprovalAsync(msg, context.CancellationToken);
                        break;

                    case MiscEnumEntity.TransactionTypeSalesQuotationAmendment:
                        await HandleSalesQuotationAmendmentApprovalAsync(msg, context.CancellationToken);
                        break;

                    case MiscEnumEntity.StoModuleTypeName:
                        await _stoHeaderCommandRepo.UpdateApprovalStatusAsync(
                            msg.ModuleTransactionId, msg.Status,
                            msg.ModifiedBy, msg.ModifiedByName, msg.ModifiedIP,
                            context.CancellationToken);
                        _logger.LogInformation(
                            "STO {Id} status updated to {Status} by user {UserId}",
                            msg.ModuleTransactionId, msg.Status, msg.ModifiedBy);
                        break;

                    case MiscEnumEntity.DCModuleTypeName:
                        await _dcCommandRepo.UpdateApprovalStatusAsync(
                            msg.ModuleTransactionId, msg.Status,
                            msg.ModifiedBy, msg.ModifiedByName, msg.ModifiedIP,
                            context.CancellationToken);
                        _logger.LogInformation(
                            "DeliveryChallan {Id} status updated to {Status} by user {UserId}",
                            msg.ModuleTransactionId, msg.Status, msg.ModifiedBy);
                        break;

                    case MiscEnumEntity.ComplaintModuleTypeName:
                        await _complaintCommandRepo.UpdateApprovalStatusAsync(
                            msg.ModuleTransactionId, msg.Status,
                            msg.ModifiedBy, msg.ModifiedByName, msg.ModifiedIP,
                            context.CancellationToken);
                        _logger.LogInformation(
                            "Complaint {Id} status updated to {Status} by user {UserId}",
                            msg.ModuleTransactionId, msg.Status, msg.ModifiedBy);

                        // ------------------- Event 2 — InApp notification: MO approved → QC team -------------------
                        // Fire only on Approved. Rejection notification is Phase 2.
                        // EventTypeId resolved at runtime via _appDataMiscLookup (Ids vary per env).
                        // Recipient resolved by dispatcher via TargetTypeId 2083 (COMPLAINT_QC_REVIEWER_USER).
                        if (msg.Status == MiscEnumEntity.ComplaintApprovalApproved)
                        {
                            try
                            {
                                var createEventType = await _appDataMiscLookup.GetMiscMasterByNameAsync(
                                    MiscEnumEntity.NotifEventTypeMiscType, MiscEnumEntity.NotifEventTypeCreate);

                                if (createEventType == null)
                                {
                                    _logger.LogWarning(
                                        "MiscMaster EventType='{Code}' not found — skipping 'Complaint MO Approval' InApp for ComplaintId {Id}",
                                        MiscEnumEntity.NotifEventTypeCreate, msg.ModuleTransactionId);
                                }
                                else
                                {
                                    var complaintUnitId = await ResolveComplaintCreatorUnitIdAsync(
                                        msg.ModuleTransactionId, context.CancellationToken);

                                    // Resolve QC-reviewer recipients dynamically (same dept-team
                                    // chain as sp_EvaluateApproval Block 4). Static WorkFlow_GetUserId
                                    // can't resolve the QC department team, so override the
                                    // dispatcher-resolved recipients in C#. Null when nothing
                                    // resolves → dispatcher fallback (no regression).
                                    var qcUserIds = await _departmentUserLookup
                                        .GetActiveUserIdsByApprovalStepTargetTypeAsync(
                                            MiscEnumEntity.ComplaintQcReviewerTargetType,
                                            context.CancellationToken);

                                    if (qcUserIds.Count > 0)
                                        _logger.LogInformation(
                                            "Resolved {Count} QC-reviewer recipient(s) for 'Complaint MO Approval' InApp, ComplaintId {Id}: [{Users}]",
                                            qcUserIds.Count, msg.ModuleTransactionId, string.Join(",", qcUserIds));
                                    else
                                        _logger.LogWarning(
                                            "No QC-reviewer recipient resolved for 'Complaint MO Approval' InApp, ComplaintId {Id} — " +
                                            "falling back to configured notification target.", msg.ModuleTransactionId);

                                    var inAppCorrelationId = Guid.NewGuid();
                                    await context.Publish(new NotificationCreatedEvent
                                    {
                                        CorrelationId = inAppCorrelationId,
                                        CreatedByName = msg.ModifiedByName ?? string.Empty,
                                        UnitId        = complaintUnitId,
                                        ModuleName    = MiscEnumEntity.NotifModuleComplaintMoApproval,
                                        EventTypeId   = createEventType.Id,
                                        Email         = string.Empty,
                                        ccMail        = string.Empty,
                                        Mobile        = string.Empty,
                                        param1        = msg.ModuleTransactionId.ToString(),
                                        param2        = string.Empty,
                                        param3        = DateTimeOffset.UtcNow,
                                        param4        = string.Empty,
                                        param5        = msg.ModifiedByName ?? string.Empty,
                                        param6        = string.Empty,
                                        param7        = string.Empty,
                                        param8        = string.Empty,
                                        param9        = string.Empty,
                                        param10       = string.Empty,
                                        OverrideTargetUserIds = qcUserIds.Count > 0 ? qcUserIds.ToList() : null,
                                        ModuleTransactionId = msg.ModuleTransactionId,
                                        ModuleTypeName = MiscEnumEntity.ComplaintModuleTypeName
                                    }, context.CancellationToken);

                                    _logger.LogInformation(
                                        "Published 'Complaint MO Approval' InApp for ComplaintId {Id}",
                                        msg.ModuleTransactionId);
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex,
                                    "Failed to publish 'Complaint MO Approval' InApp for ComplaintId {Id}",
                                    msg.ModuleTransactionId);
                            }
                        }
                        else if (msg.Status == MiscEnumEntity.ComplaintApprovalRejected)
                        {
                            // Phase 3: bell goes to the complaint creator (the agent/CSR
                            // who logged the complaint) so they know it's been bounced back.
                            var complaint = await _complaintQueryRepo.GetByIdAsync(msg.ModuleTransactionId);
                            var creatorIds = complaint != null && complaint.CreatedBy > 0
                                ? new List<int> { complaint.CreatedBy }
                                : null;
                            await PublishComplaintInAppAsync(
                                msg.ModuleTransactionId,
                                MiscEnumEntity.NotifModuleComplaintRejected,
                                MiscEnumEntity.ComplaintModuleTypeName,
                                creatorIds,
                                msg.ModifiedByName ?? string.Empty,
                                context);
                        }
                        break;

                    case MiscEnumEntity.ComplaintQCReviewModuleTypeName:
                        await _complaintCommandRepo.UpdateQCReviewApprovalStatusAsync(
                            msg.ModuleTransactionId, msg.Status,
                            msg.ModifiedBy, msg.ModifiedByName, msg.ModifiedIP,
                            context.CancellationToken);
                        _logger.LogInformation(
                            "Complaint QC Review {Id} approval status updated to {Status} by user {UserId}",
                            msg.ModuleTransactionId, msg.Status, msg.ModifiedBy);

                        // Auto-seed a draft ComplaintResolution row when QC lands on 'QC Accepted'.
                        // Pass the approver as the creator so the audit trail shows the human user.
                        await _complaintCommandRepo.EnsureResolutionDraftIfQCAcceptedAsync(
                            msg.ModuleTransactionId,
                            msg.ModifiedBy, msg.ModifiedByName, msg.ModifiedIP,
                            context.CancellationToken);

                        if (msg.Status == MiscEnumEntity.ComplaintApprovalApproved)
                        {
                            // Phase 3: bell goes to the agent-MO chain — the MO whose
                            // complaint moves to the Resolution stage on QC approval.
                            var agentIds = await _complaintQueryRepo
                                .GetComplaintAgentIdsAsync(msg.ModuleTransactionId);
                            var moUserIds = new List<int>();
                            if (agentIds.Count > 0)
                            {
                                var moChain = await _officerAgentUserLookup
                                    .GetMarketingOfficerChainByAgentIdsAsync(agentIds, context.CancellationToken);
                                moUserIds = moChain
                                    .Where(r => r.MoUserId.HasValue && r.MoUserId.Value > 0)
                                    .Select(r => r.MoUserId!.Value)
                                    .Distinct()
                                    .ToList();
                            }
                            await PublishComplaintInAppAsync(
                                msg.ModuleTransactionId,
                                MiscEnumEntity.NotifModuleQcReviewApproved,
                                MiscEnumEntity.ComplaintQCReviewModuleTypeName,
                                moUserIds.Count > 0 ? moUserIds : null,
                                msg.ModifiedByName ?? string.Empty,
                                context);
                        }
                        else if (msg.Status == MiscEnumEntity.ComplaintApprovalRejected)
                        {
                            // Phase 3: bell goes to the QC reviewer who submitted this
                            // review (ComplaintQCReview.ReviewedBy) — they need to know
                            // their review was bounced back.
                            var qc = await _qcReviewQueryRepo.GetByComplaintIdAsync(msg.ModuleTransactionId);
                            var reviewerIds = qc?.ReviewedBy is > 0
                                ? new List<int> { qc.ReviewedBy!.Value }
                                : null;
                            await PublishComplaintInAppAsync(
                                msg.ModuleTransactionId,
                                MiscEnumEntity.NotifModuleQcReviewRejected,
                                MiscEnumEntity.ComplaintQCReviewModuleTypeName,
                                reviewerIds,
                                msg.ModifiedByName ?? string.Empty,
                                context);
                        }
                        break;

                    case MiscEnumEntity.ComplaintResolutionModuleTypeName:
                        await _complaintCommandRepo.UpdateResolutionApprovalStatusAsync(
                            msg.ModuleTransactionId, msg.Status,
                            msg.ModifiedBy, msg.ModifiedByName, msg.ModifiedIP,
                            context.CancellationToken);
                        _logger.LogInformation(
                            "Complaint Resolution {Id} approval status updated to {Status} by user {UserId}",
                            msg.ModuleTransactionId, msg.Status, msg.ModifiedBy);

                        if (msg.Status == MiscEnumEntity.ComplaintApprovalApproved)
                        {
                            // Phase 4: end-of-loop bell — Resolution has been fully approved,
                            // so the complaint cycle is closed. Bell goes to the complaint
                            // creator (the user who originally lodged the complaint) so they
                            // see the resolution outcome. Symmetric to Complaint Rejected.
                            var complaint = await _complaintQueryRepo.GetByIdAsync(msg.ModuleTransactionId);
                            var creatorIds = complaint != null && complaint.CreatedBy > 0
                                ? new List<int> { complaint.CreatedBy }
                                : null;
                            await PublishComplaintInAppAsync(
                                msg.ModuleTransactionId,
                                MiscEnumEntity.NotifModuleResolutionApproved,
                                MiscEnumEntity.ComplaintResolutionModuleTypeName,
                                creatorIds,
                                msg.ModifiedByName ?? string.Empty,
                                context);
                        }
                        else if (msg.Status == MiscEnumEntity.ComplaintApprovalRejected)
                        {
                            // Phase 3: bell goes to the MO who submitted this resolution
                            // (ComplaintResolution.ResolvedBy) so they can revise + resubmit.
                            var res = await _resolutionQueryRepo.GetByComplaintHeaderIdAsync(msg.ModuleTransactionId);
                            var resolverIds = res?.ResolvedBy is > 0
                                ? new List<int> { res.ResolvedBy!.Value }
                                : null;
                            await PublishComplaintInAppAsync(
                                msg.ModuleTransactionId,
                                MiscEnumEntity.NotifModuleResolutionRejected,
                                MiscEnumEntity.ComplaintResolutionModuleTypeName,
                                resolverIds,
                                msg.ModifiedByName ?? string.Empty,
                                context);
                        }
                        break;

                    default:
                        _logger.LogWarning("Unknown Sales ModuleTypeName: {Type}", msg.ModuleTypeName);
                        return;
                }

                // Publish completion event
                await context.Publish(new ApprovedRejectedSalesCompletedEvent
                {
                    CorrelationId = msg.CorrelationId,
                    ModuleTransactionId = msg.ModuleTransactionId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "ApprovedRejectedSalesConsumer failed for TransactionId={Id}",
                    msg.ModuleTransactionId);
                throw; // MassTransit retry policy handles retries
            }
        }

        private async Task HandleSalesOrderAmendmentApprovalAsync(
            UpdateApprovedRejectedSalesCommand msg, CancellationToken ct)
        {
            _logger.LogInformation(
                "SalesOrderAmendment Approval: Id={Id}, Status={Status}",
                msg.ModuleTransactionId, msg.Status);

            var status = msg.Status;
            if (status != MiscEnumEntity.SalesOrderStatusApproved &&
                status != MiscEnumEntity.SalesOrderStatusRejected)
                return;

            var result = await _amendmentCommandRepo.ApplyAmendmentAsync(
                msg.ModuleTransactionId, status,
                msg.ModifiedBy, msg.ModifiedByName, msg.ModifiedIP, ct);

            if (!result)
            {
                _logger.LogWarning(
                    "SalesOrderAmendment not found or already processed: Id={Id}",
                    msg.ModuleTransactionId);
                return;
            }

            _logger.LogInformation(
                "SalesOrderAmendment Id={Id} status updated to {Status}",
                msg.ModuleTransactionId, status);
        }

        private async Task HandleSalesQuotationAmendmentApprovalAsync(
            UpdateApprovedRejectedSalesCommand msg, CancellationToken ct)
        {
            _logger.LogInformation(
                "SalesQuotationAmendment Approval: Id={Id}, Status={Status}",
                msg.ModuleTransactionId, msg.Status);

            var status = msg.Status;
            if (status != MiscEnumEntity.SalesOrderStatusApproved &&
                status != MiscEnumEntity.SalesOrderStatusRejected)
                return;

            var result = await _sqAmendmentCommandRepo.ApplyAmendmentAsync(
                msg.ModuleTransactionId, status,
                msg.ModifiedBy, msg.ModifiedByName, msg.ModifiedIP, ct);

            if (!result)
            {
                _logger.LogWarning(
                    "SalesQuotationAmendment not found or already processed: Id={Id}",
                    msg.ModuleTransactionId);
                return;
            }

            _logger.LogInformation(
                "SalesQuotationAmendment Id={Id} status updated to {Status}",
                msg.ModuleTransactionId, status);
        }

        private async Task HandleSalesQuotationApprovalAsync(
            UpdateApprovedRejectedSalesCommand msg, CancellationToken ct)
        {
            _logger.LogInformation(
                "SalesQuotation Approval: Id={Id}, Status={Status}",
                msg.ModuleTransactionId, msg.Status);

            var status = msg.Status;
            var quotationId = msg.ModuleTransactionId;

            if (status != MiscEnumEntity.SalesOrderStatusApproved &&
                status != MiscEnumEntity.SalesOrderStatusRejected)
                return;

            // Resolve Approved and Rejected MiscMaster Ids
            var statusApproved = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.SalesOrderApprovalStatus, MiscEnumEntity.SalesOrderStatusApproved);
            var statusRejected = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.SalesOrderApprovalStatus, MiscEnumEntity.SalesOrderStatusRejected);

            var finalStatusId = status == MiscEnumEntity.SalesOrderStatusApproved
                ? statusApproved?.Id ?? 0
                : statusRejected?.Id ?? 0;

            var quotation = await _sqCommandRepo.GetByIdEntityAsync(quotationId);
            if (quotation == null)
            {
                _logger.LogWarning("Sales Quotation not found for Id={QuotationId}", quotationId);
                return;
            }

            quotation.StatusId = finalStatusId;
            await _sqCommandRepo.FinalizeQuotationStatusAsync(quotation);

            _logger.LogInformation(
                "SalesQuotation Id={QuotationId} status updated to {Status} (StatusId={StatusId})",
                quotationId, status, finalStatusId);
        }

        private async Task HandleInvoiceApprovalAsync(
            UpdateApprovedRejectedSalesCommand msg,
            List<JsonElement> dynamicFields,
            CancellationToken ct)
        {
            // 1. Always update Invoice approval status
            await _invoiceCommandRepo.UpdateApprovalStatusAsync(
                msg.ModuleTransactionId, msg.Status,
                msg.ModifiedBy, msg.ModifiedByName, msg.ModifiedIP, ct);

            // 2. Only proceed with EInvoice on Approval (not Rejection)
            if (msg.Status != MiscEnumEntity.InvoiceStatusApproved)
                return;

            // 3. Parse DynamicFields for withInvoice / withEwaybill flags
            bool withInvoice = ReadBool(dynamicFields, "withInvoice");
            bool withEwaybill = ReadBool(dynamicFields, "withEwaybill");

            if (!withInvoice)
            {
                _logger.LogInformation(
                    "Invoice {Id} approved. withInvoice=false, skipping EInvoice creation.",
                    msg.ModuleTransactionId);
                return;
            }

            // 4. Create EInvoice via Finance module (Contracts command → Finance handler)
            try
            {
                var command = new CreateEInvoiceFromSalesCommand
                {
                    InvoiceId = msg.ModuleTransactionId,
                    IsEwaybillCreate = withEwaybill
                };

                var result = await _mediator.Send(command, ct);

                if (!result.IsSuccess)
                {
                    throw new InvalidOperationException(
                        $"EInvoice creation failed for Invoice {msg.ModuleTransactionId}: {result.Message}");
                }

                _logger.LogInformation(
                    "EInvoice created for Invoice {InvoiceId}: EInvoiceHeaderId={EInvoiceId}, IRN={Irn}, EwbNo={EwbNo}",
                    msg.ModuleTransactionId,
                    result.Data?.EInvoiceHeaderId,
                    result.Data?.Irn,
                    result.Data?.EwbNo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "EInvoice creation failed for Invoice {Id}, rolling back to Pending",
                    msg.ModuleTransactionId);

                await _invoiceCommandRepo.UpdateApprovalStatusAsync(
                    msg.ModuleTransactionId,
                    MiscEnumEntity.InvoiceStatusPending,
                    msg.ModifiedBy, msg.ModifiedByName, msg.ModifiedIP,
                    ct);
            }
        }

        private async Task HandleSalesOrderApprovalAsync(
            ConsumeContext<UpdateApprovedRejectedSalesCommand> context)
        {
            var msg = context.Message;
            var ct = context.CancellationToken;

            _logger.LogInformation(
                "SalesOrder Consumer Approval Status Update: ModuleTransactionId={Id}, Status={Status}",
                msg.ModuleTransactionId, msg.Status);

            var status = msg.Status;
            var salesOrderId = msg.ModuleTransactionId;

            if (msg.ModuleTypeName != MiscEnumEntity.TransactionTypeSalesOrder)
                return;

            // Allow Approved, Rejected AND Pending. "Pending" = an intermediate approval was
            // taken (e.g., MO approved) but the workflow still has more approvers to go.
            // Step-2 escalation needs this branch to fire on intermediate approvals.
            if (status != MiscEnumEntity.SalesOrderStatusApproved &&
                status != MiscEnumEntity.SalesOrderStatusRejected &&
                status != MiscEnumEntity.SalesOrderStatusPending)
                return;

            var order = await _salesOrderCommandRepo.GetByIdEntityAsync(salesOrderId);
            if (order == null)
            {
                _logger.LogWarning("Sales Order not found for Id={SalesOrderId}", salesOrderId);
                return;
            }

            // Persist status change ONLY for terminal states (Approved / Rejected). For Pending
            // the SO is already Pending — no DB write needed; we just continue to Step-2 below.
            if (status == MiscEnumEntity.SalesOrderStatusApproved ||
                status == MiscEnumEntity.SalesOrderStatusRejected)
            {
                var statusApproved = await _miscMasterQueryRepository.GetMiscMasterByName(
                    MiscEnumEntity.SalesOrderApprovalStatus, MiscEnumEntity.SalesOrderStatusApproved);

                var statusRejected = await _miscMasterQueryRepository.GetMiscMasterByName(
                    MiscEnumEntity.SalesOrderApprovalStatus, MiscEnumEntity.SalesOrderStatusRejected);

                var finalStatusId = status == MiscEnumEntity.SalesOrderStatusApproved
                    ? statusApproved?.Id ?? 0
                    : statusRejected?.Id ?? 0;

                order.StatusId = finalStatusId;

                await _salesOrderCommandRepo.FinalizeOrderStatusAsync(
                    order, msg.ModifiedBy, msg.ModifiedByName, msg.ModifiedIP);

                _logger.LogInformation(
                    "SalesOrder Id={SalesOrderId} status updated to {Status} (StatusId={StatusId})",
                    salesOrderId, status, finalStatusId);
            }

            // ── Steps 2 & 3: Workflow escalation on Pending ──
            // Determine which step the approver is at, by comparing msg.ModifiedBy against the
            // agent's MO and the MO's ReportTo (HOD).
            //
            //   Step 2: MO approved   → notify HOD ............ ModuleName="Sales Order MO Approval"
            //   Step 3: HOD approved  → notify MD (HOD's ReportTo)
            //                          BUT only when order.IsMdDiscountEnabled = true
            //                          ModuleName="Sales Order MD Approval"
            //
            // Skipped silently in all other cases (rejected, approved-final, approver isn't
            // MO/HOD, MdDiscount disabled at Step 3).
            if (status == MiscEnumEntity.SalesOrderStatusPending && msg.ModifiedBy > 0 && order.AgentId > 0)
            {
                try
                {
                    var moUserId = await _officerAgentUserLookup
                        .GetMarketingOfficerUserIdByAgentIdAsync(order.AgentId.Value, ct);

                    var hodUserId = (moUserId.HasValue && moUserId.Value > 0)
                        ? await _officerAgentUserLookup.GetMarketingOfficerReportToUserIdAsync(moUserId.Value, ct)
                        : null;

                    string?  moduleName        = null;
                    int?     nextRecipientId   = null;
                    string   stepLabel         = "";

                    if (moUserId.HasValue && msg.ModifiedBy == moUserId.Value)
                    {
                        // Step 2: MO just approved → recipient is HOD
                        nextRecipientId = hodUserId;
                        moduleName      = "Sales Order MO Approval";
                        stepLabel       = "Step-2";
                    }
                    else if (hodUserId.HasValue && msg.ModifiedBy == hodUserId.Value)
                    {
                        // Step 3: HOD just approved → only escalate to MD if MdDiscount is enabled.
                        // The MD's UserId comes from AppData.ApprovalRequest (workflow's authoritative
                        // source — keyed by ApprovalRuleId = 1), not from Users.ReportToId.
                        if (!order.IsMdDiscountEnabled)
                        {
                            _logger.LogInformation(
                                "Step-3 InApp skipped. SalesOrder {SalesOrderNo} (Id={Id}) HOD approval but " +
                                "IsMdDiscountEnabled=false — workflow ends here, no MD escalation.",
                                order.SalesOrderNo, salesOrderId);
                            return;
                        }

                        // MD-level approver is registered by the workflow engine in
                        // AppData.ApprovalRequest with ApprovalRuleId = 1 (the rule-conditional
                        // step that activates when MdDiscount is enabled).
                        nextRecipientId = await _workflowLookup.GetApproverUserIdByRuleAsync(
                            workflowType:        MiscEnumEntity.TransactionTypeSalesOrder,
                            moduleTransactionId: salesOrderId,
                            unitId:              order.OrderUnitId ?? 0,
                            approvalRuleId:      1,
                            cancellationToken:   ct);
                        moduleName      = "Sales Order MD Approval";
                        stepLabel       = "Step-3";
                    }
                    else
                    {
                        _logger.LogInformation(
                            "Pending escalation skipped. Approver UserId={ApproverId} is neither the agent's MO " +
                            "(UserId={MoId}) nor the HOD (UserId={HodId}). SalesOrder Id={Id}",
                            msg.ModifiedBy, moUserId, hodUserId, salesOrderId);
                        return;
                    }

                    if (!nextRecipientId.HasValue || nextRecipientId.Value <= 0)
                    {
                        _logger.LogInformation(
                            "{Step} InApp skipped. Could not resolve next-level recipient for ApproverId={ApproverId} " +
                            "on SalesOrder Id={Id}.",
                            stepLabel, msg.ModifiedBy, salesOrderId);
                        return;
                    }

                    var notifEventMisc = await _appDataMiscLookup.GetMiscMasterByNameAsync(
                        NotificationEnum.NotificationEvent, NotificationEnum.Create);

                    var customer = order.PartyId > 0
                        ? await _partyDetailLookup.GetByIdAsync(order.PartyId, ct)
                        : null;
                    var customerName = customer?.PartyName ?? "";

                    var inAppCorrelationId = Guid.NewGuid();
                    var inAppEvent = new NotificationCreatedEvent
                    {
                        CorrelationId         = inAppCorrelationId,
                        CreatedByName         = msg.ModifiedByName ?? string.Empty,
                        UnitId                = order.OrderUnitId ?? 0,
                        ModuleName            = moduleName,
                        EventTypeId           = notifEventMisc?.Id ?? 0,
                        Email                 = "",
                        ccMail                = "",
                        Mobile                = "",
                        param1                = order.SalesOrderNo ?? "",
                        param2                = customerName,
                        param3                = DateTimeOffset.UtcNow,
                        param4                = "",
                        param5                = "",
                        param6                = "",
                        param7                = "",
                        param8                = "",
                        param9                = "",
                        param10               = "",
                        OverrideTargetUserIds = new List<int> { nextRecipientId.Value },
                        ModuleTransactionId   = salesOrderId,
                        ModuleTypeName        = MiscEnumEntity.TransactionTypeSalesOrder
                    };

                    // Direct publish via consumer's bus context — same pattern as the completion event.
                    await context.Publish(inAppEvent, ct);

                    _logger.LogInformation(
                        "{Step} InApp published. SalesOrder {SalesOrderNo} (Id={Id}) approved by UserId={ApproverId}; " +
                        "ModuleName={ModuleName}, escalation recipient UserId={RecipientId} (CorrId: {Corr})",
                        stepLabel, order.SalesOrderNo, salesOrderId, msg.ModifiedBy,
                        moduleName, nextRecipientId.Value, inAppCorrelationId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Failed to publish escalation InApp for SalesOrder Id={Id} approved by UserId={ApproverId}",
                        salesOrderId, msg.ModifiedBy);
                    // Do NOT rethrow — the SO approval itself succeeded; notification is best-effort.
                }
            }
        }

        // Resolves the unit of the complaint's creator so the InApp notification routes to the
        // correct unit's QC team. Falls back to 0 (with a warning) when the complaint can't be
        // found or the creator has no unit assignment — the dispatcher will then drop the message
        // rather than route it to an arbitrary unit.
        private async Task<int> ResolveComplaintCreatorUnitIdAsync(int complaintId, CancellationToken ct)
        {
            var complaint = await _complaintQueryRepo.GetByIdAsync(complaintId);
            if (complaint == null)
            {
                _logger.LogWarning(
                    "Cannot resolve UnitId for ComplaintId {Id} — complaint not found",
                    complaintId);
                return 0;
            }

            var units = await _unitLookup.GetUserUnitAsync(complaint.CreatedBy);
            var unitId = units.FirstOrDefault()?.UnitId ?? 0;

            if (unitId == 0)
            {
                _logger.LogWarning(
                    "Cannot resolve UnitId for ComplaintId {Id} — creator UserId={CreatedBy} has no unit assignment",
                    complaintId, complaint.CreatedBy);
            }

            return unitId;
        }

        // Shared publish helper for the Phase 3 dynamic-recipient complaint notifications
        // (Complaint Rejected / QC Review Approved+Rejected / Resolution Rejected). Resolves
        // the EventType + creator's unit, publishes a NotificationCreatedEvent with the
        // pre-resolved OverrideTargetUserIds, and never throws (notification failure must not
        // fail the approval pipeline).
        private async Task PublishComplaintInAppAsync(
            int complaintId,
            string moduleName,
            string moduleTypeName,
            List<int>? overrideUserIds,
            string approverName,
            ConsumeContext<UpdateApprovedRejectedSalesCommand> context)
        {
            try
            {
                var createEventType = await _appDataMiscLookup.GetMiscMasterByNameAsync(
                    MiscEnumEntity.NotifEventTypeMiscType, MiscEnumEntity.NotifEventTypeCreate);

                if (createEventType == null)
                {
                    _logger.LogWarning(
                        "MiscMaster EventType='{Code}' not found — skipping '{Module}' InApp for ComplaintId {Id}",
                        MiscEnumEntity.NotifEventTypeCreate, moduleName, complaintId);
                    return;
                }

                var unitId = await ResolveComplaintCreatorUnitIdAsync(complaintId, context.CancellationToken);
                var inAppCorrelationId = Guid.NewGuid();

                await context.Publish(new NotificationCreatedEvent
                {
                    CorrelationId = inAppCorrelationId,
                    CreatedByName = approverName,
                    UnitId        = unitId,
                    ModuleName    = moduleName,
                    EventTypeId   = createEventType.Id,
                    Email         = string.Empty,
                    ccMail        = string.Empty,
                    Mobile        = string.Empty,
                    param1        = complaintId.ToString(),
                    param2        = string.Empty,
                    param3        = DateTimeOffset.UtcNow,
                    param4        = string.Empty,
                    param5        = approverName,
                    param6        = string.Empty,
                    param7        = string.Empty,
                    param8        = string.Empty,
                    param9        = string.Empty,
                    param10       = string.Empty,
                    OverrideTargetUserIds = overrideUserIds,
                    ModuleTransactionId = complaintId,
                    ModuleTypeName = moduleTypeName
                }, context.CancellationToken);

                _logger.LogInformation(
                    "Published '{Module}' InApp for ComplaintId {Id} to {Count} user(s) [{Users}]",
                    moduleName, complaintId,
                    overrideUserIds?.Count ?? 0,
                    overrideUserIds == null ? "fallback" : string.Join(",", overrideUserIds));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to publish '{Module}' InApp for ComplaintId {Id}", moduleName, complaintId);
            }
        }

        /// <summary>
        /// Reads a boolean value from the first DynamicFields JsonElement that contains the property.
        /// Expected format: [{ "withInvoice": true, "withEwaybill": false }]
        /// </summary>
        private static bool ReadBool(List<JsonElement> dynamicFields, string propertyName)
        {
            foreach (var element in dynamicFields)
            {
                if (element.ValueKind == JsonValueKind.Object &&
                    element.TryGetProperty(propertyName, out var prop))
                {
                    if (prop.ValueKind == JsonValueKind.True)
                        return true;
                    if (prop.ValueKind == JsonValueKind.False)
                        return false;
                    // Handle string "true"/"false"
                    if (prop.ValueKind == JsonValueKind.String &&
                        bool.TryParse(prop.GetString(), out var parsed))
                        return parsed;
                }
            }
            return false;
        }
    }
}
