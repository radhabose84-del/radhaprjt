using System.Text.Json;
using AutoMapper;
using BackgroundService.Application.Interfaces.IMiscMaster;
using Contracts.Interfaces;
using BackgroundService.Application.Notification.Common.Interfaces;
using BackgroundService.Application.Workflow.Common.Interfaces.IApprovalRequest;
using BackgroundService.Domain.Common;
using BackgroundService.Domain.Entities.Workflow;
using Contracts.Dtos.Purchase;          // ✅ For UpdateLineStatusDto
using Contracts.Events.Workflow;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BackgroundService.Application.Workflow.ApprovalRequests.Commands.ApproveApprovalRequest
{
    public class ApproveApprovalRequestCommandHandler : IRequestHandler<ApproveApprovalRequestCommand, bool>
    {
        private readonly IMiscMasterQueryRepository _miscMasterQuery;
        private readonly IIPAddressService _ipAddressService;
        private readonly ITimeZoneService _timeZoneService;
        private readonly IApprovalRequestCommand _approvalRequestCommand;
        private readonly IApprovalRequestQuery _approvalRequestQuery;
        private readonly IEventPublisher _eventPublisher;
        private readonly IMapper _imapper;
        private readonly ILogger<ApproveApprovalRequestCommandHandler> _logger;

        public ApproveApprovalRequestCommandHandler(
            IMiscMasterQueryRepository miscMasterQuery,
            IIPAddressService ipAddressService,
            ITimeZoneService timeZoneService,
            IApprovalRequestCommand approvalRequestCommand,
            IApprovalRequestQuery approvalRequestQuery,
            IEventPublisher eventPublisher,
            IMapper imapper,
            ILogger<ApproveApprovalRequestCommandHandler> logger)
        {
            _miscMasterQuery = miscMasterQuery;
            _ipAddressService = ipAddressService;
            _timeZoneService = timeZoneService;
            _approvalRequestCommand = approvalRequestCommand;
            _approvalRequestQuery = approvalRequestQuery;
            _eventPublisher = eventPublisher;
            _imapper = imapper;
            _logger = logger;
        }

        public async Task<bool> Handle(ApproveApprovalRequestCommand request, CancellationToken cancellationToken)
        {
            if (request.ModuleTransactionId <= 0)
                throw new InvalidOperationException("ModuleTransactionId is required.");

            // Prevent double-approval: check current aggregate status before proceeding
            var currentStatus = await _approvalRequestQuery.HeaderLevelApprovalStatus(
                request.ApprovalRequestHeaderId,
                request.ModuleTransactionId);
            var currentStatusMap = _imapper.Map<HeaderStatusDto>(currentStatus);
            if (currentStatusMap?.StatusCode == MiscEnumEntity.Approved)
                throw new InvalidOperationException("This Id is already approved.");

            var statusApproved = await _miscMasterQuery.GetMiscMasterByName(MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Approved);
            var statusRejected = await _miscMasterQuery.GetMiscMasterByName(MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Rejected);

            string currentIp = _ipAddressService.GetSystemIPAddress();
            int userId = _ipAddressService.GetUserId();
            string username = _ipAddressService.GetUserName();
            var systemTimeZoneId = _timeZoneService.GetSystemTimeZone();
            var currentTime = _timeZoneService.GetCurrentTime(systemTimeZoneId);

            _logger.LogInformation("Approval Request: {@Request}", request);

            var approvalReq = _imapper.Map<ApprovalRequest>(request);
            approvalReq.ModifiedIP = currentIp;
            approvalReq.ModifiedDate = currentTime;
            approvalReq.ModifiedBy = userId;
            approvalReq.ModifiedByName = username;

            var isLineLevelApproval = await _approvalRequestQuery.IsLineLevelApproval(request.ApprovalRequestHeaderId);

            // =========================
            // LINE LEVEL APPROVAL
            // =========================
            if (isLineLevelApproval)
            {
                List<ApproveLineStatusDto> lineStatus;

                if (request.ApprovalRequestLine != null && request.ApprovalRequestLine.Any())
                {
                    // ✅ CASE 1: UI sent specific lines
                    lineStatus = _imapper.Map<List<ApproveLineStatusDto>>(request.ApprovalRequestLine);

                    foreach (var line in lineStatus)
                        line.NewStatusId = line.IsApproved == 1 ? statusApproved.Id : statusRejected.Id;
                }
                else
                {
                    // ✅ CASE 2: UI sent NO lines -> approve/reject ALL lines for this header
                    var allLines = await _approvalRequestQuery.GetApprovalRequestLinesAsync(
                        request.ApprovalRequestHeaderId,
                        cancellationToken);

                    lineStatus = allLines.Select(l => new ApproveLineStatusDto
                    {
                        ApprovalRequestLineId   = l.ApprovalRequestLineId,      
                        ModuleLineTransactionId = l.ModuleLineTransactionId,
                        NewStatusId = request.IsApproved == 1
                            ? statusApproved.Id
                            : statusRejected.Id
                    }).ToList();
                }

                // 🔁 Call SP – now JSON has correct ApprovalRequestLineId values
                await _approvalRequestCommand.Approve(
                    approvalReq,
                    JsonSerializer.Serialize(lineStatus),
                    cancellationToken);

                // 🔁 Re-read final statuses for event
                var (lineStatusDbAfter, headerStatusDbAfter) =
                    await _approvalRequestQuery.GetApprovalRequestById(
                        request.ApprovalRequestHeaderId,
                        request.ModuleTransactionId);

                var lineStatusMap = _imapper.Map<List<LineStatusDto>>(lineStatusDbAfter);
                var headerStatusMapLine = _imapper.Map<HeaderStatusDto>(headerStatusDbAfter);

                List<UpdateLineStatusDto> approvalReqLine;

                if (request.ApprovalRequestLine != null && request.ApprovalRequestLine.Any())
                {
                    // Only UI lines
                    approvalReqLine = _imapper.Map<List<UpdateLineStatusDto>>(request.ApprovalRequestLine);
                    var lookup = lineStatusMap.ToDictionary(d => d.ModuleLineTransactionId, d => d.Status);

                    foreach (var item in approvalReqLine)
                        if (lookup.TryGetValue(item.ModuleLineId, out var st))
                            item.Status = st;
                }
                else
                {
                    // UI empty → send ALL lines to downstream (Purchase/Budget/etc.)
                    approvalReqLine = lineStatusMap.Select(d => new UpdateLineStatusDto
                    {
                        ModuleLineId = d.ModuleLineTransactionId,
                        Status       = d.Status
                    }).ToList();
                }

                var evLine = new ApprovedRejectedEvent
                {
                    CorrelationId       = Guid.NewGuid(),
                    ModuleTransactionId = request.ModuleTransactionId,
                    LineStatus          = approvalReqLine,
                    ModuleTypeName      = headerStatusMapLine.WorkflowType,
                    Status              = headerStatusMapLine.StatusCode,
                    PartyContacts       = request.PartyContacts ?? new(),
                    DynamicFields       = NormalizeDynamicFields(request.DynamicFields),
                    ModifiedBy          = userId,
                    ModifiedByName      = username,
                    ModifiedIP          = currentIp
                };

                await _eventPublisher.SaveEventAsync(evLine);
                await _eventPublisher.PublishPendingEventsAsync();
                return true;
            }

            // =========================
            // HEADER LEVEL APPROVAL
            // =========================
            approvalReq.StatusId = request.IsApproved == 1 ? statusApproved.Id : statusRejected.Id;

            await _approvalRequestCommand.Approve(approvalReq, cancellationToken);

            var headerStatusDbHeader =
                await _approvalRequestQuery.HeaderLevelApprovalStatus(
                    request.ApprovalRequestHeaderId,
                    request.ModuleTransactionId);

            var headerStatusMapHeader = _imapper.Map<HeaderStatusDto>(headerStatusDbHeader);

            var evHeader = new ApprovedRejectedEvent
            {
                CorrelationId       = Guid.NewGuid(),
                ModuleTransactionId = request.ModuleTransactionId,
                ModuleTypeName      = headerStatusMapHeader.WorkflowType,
                Status              = headerStatusMapHeader.StatusCode,
                LineStatus          = new List<UpdateLineStatusDto>(),
                PartyContacts       = request.PartyContacts ?? new(),
                DynamicFields       = NormalizeDynamicFields(request.DynamicFields),
                ModifiedBy          = userId,
                ModifiedByName      = username,
                ModifiedIP          = currentIp
            };

            await _eventPublisher.SaveEventAsync(evHeader);
            await _eventPublisher.PublishPendingEventsAsync();
            return true;
        }

        private static List<JsonElement> NormalizeDynamicFields(List<JsonElement>? fields)
        {
            if (fields == null || fields.Count == 0) return new();

            if (fields.Count == 1 &&
                fields[0].ValueKind == JsonValueKind.String &&
                fields[0].GetString() == "string")
                return new();

            return fields;
        }
    }
}
