using AutoMapper;
using Contracts.Commands.Workflow;
using Contracts.Common;
using Contracts.Events.Notifications;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Common;
using Contracts.Interfaces.Lookups.Finance;
using Contracts.Interfaces.Lookups.Sales;
using MediatR;
using Microsoft.Extensions.Logging;
using SalesManagement.Application.Common.Interfaces.IComplaint;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Application.Common.Interfaces.IOutbox;
using SalesManagement.Application.Complaint.Dto;
using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using SalesManagement.Domain.Events;
using System.Text.Json;

namespace SalesManagement.Application.Complaint.Commands.CreateComplaint
{
    public class CreateComplaintCommandHandler : IRequestHandler<CreateComplaintCommand, ApiResponseDTO<int>>
    {
        private readonly IComplaintCommandRepository _commandRepository;
        private readonly IComplaintQueryRepository _queryRepository;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
        private readonly IDocumentSequenceLookup _documentSequenceLookup;
        private readonly IIPAddressService _ipAddressService;
        private readonly IOutboxEventPublisher _outboxEventPublisher;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateComplaintCommandHandler> _logger;
        private readonly IAppDataMiscMasterLookup _appDataMiscLookup;
        private readonly IOfficerAgentUserLookup _officerAgentUserLookup;

        public CreateComplaintCommandHandler(
            IComplaintCommandRepository commandRepository,
            IComplaintQueryRepository queryRepository,
            IMiscMasterQueryRepository miscMasterQueryRepository,
            IDocumentSequenceLookup documentSequenceLookup,
            IIPAddressService ipAddressService,
            IOutboxEventPublisher outboxEventPublisher,
            IMediator mediator,
            IMapper mapper,
            ILogger<CreateComplaintCommandHandler> logger,
            IAppDataMiscMasterLookup appDataMiscLookup,
            IOfficerAgentUserLookup officerAgentUserLookup)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _miscMasterQueryRepository = miscMasterQueryRepository;
            _documentSequenceLookup = documentSequenceLookup;
            _ipAddressService = ipAddressService;
            _outboxEventPublisher = outboxEventPublisher;
            _mediator = mediator;
            _mapper = mapper;
            _logger = logger;
            _appDataMiscLookup = appDataMiscLookup;
            _officerAgentUserLookup = officerAgentUserLookup;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateComplaintCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<ComplaintHeader>(request);

            // Set Pending status from ApprovalStatus
            var pendingStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.ComplaintApprovalStatus, MiscEnumEntity.ComplaintApprovalPending);
            entity.StatusId = pendingStatus?.Id ?? 0;

            // Map details
            if (request.Details != null && request.Details.Count > 0)
            {
                entity.ComplaintDetails = new List<ComplaintDetail>();
                foreach (var detail in request.Details)
                {
                    var detailEntity = _mapper.Map<ComplaintDetail>(detail);

                    // Map nature of complaint IDs to junction entities
                    if (detail.NatureOfComplaintIds != null && detail.NatureOfComplaintIds.Count > 0)
                    {
                        detailEntity.ComplaintDetailNatures = detail.NatureOfComplaintIds
                            .Select(natureId => new ComplaintDetailNature
                            {
                                NatureOfComplaintId = natureId
                            }).ToList();
                    }

                    entity.ComplaintDetails.Add(detailEntity);
                }
            }

            // Get UnitId from JWT token
            var unitId = _ipAddressService.GetUnitId();

            // Generate Complaint Number from Finance.DocumentSequence
            var typeId = await _documentSequenceLookup.GetTransactionTypeIdAsync(
                "Complaint", "Sales", unitId ?? 0);
            if (!typeId.HasValue)
                throw new ExceptionRules("Transaction Type 'Complaint' not found for Sales module.");

            var sequences = await _documentSequenceLookup.GenerateDocumentNumber(typeId.Value);
            var complaintNumber = sequences.Count > 0 ? sequences[^1] : null;
            entity.ComplaintNumber = complaintNumber
                ?? throw new ExceptionRules("No document sequence configured for Complaint.");

            var newId = await _commandRepository.CreateAsync(entity, typeId.Value);

            // ------------------- Event 1 — InApp notification: Complaint raised → Agent's MO -------------------
            // ModuleName='New Complaint'. The notification dispatcher (WorkFlow_GetUserId)
            // does NOT dynamically resolve the complaint's agent MO — it would fall back to
            // the static NotificationLevelHierarchy.TargetId. So we resolve the real
            // recipient(s) here (same chain as sp_EvaluateApproval:
            // ComplaintHeader → ComplaintDetail → InvoiceHeader → DispatchAdviceHeader →
            // SalesOrderHeader.AgentId → OfficerAgent → User) and pass them via
            // OverrideTargetUserIds. If resolution yields nobody, OverrideTargetUserIds is
            // left unset and the dispatcher falls back to its configured target (no regression).
            try
            {
                var createEventType = await _appDataMiscLookup.GetMiscMasterByNameAsync(
                    MiscEnumEntity.NotifEventTypeMiscType, MiscEnumEntity.NotifEventTypeCreate);

                if (createEventType == null)
                {
                    _logger.LogWarning(
                        "MiscMaster EventType='{Code}' not found — skipping 'New Complaint' InApp for Complaint {Id}",
                        MiscEnumEntity.NotifEventTypeCreate, newId);
                }
                else
                {
                    // Dynamically resolve the complaint's Agent's MO user(s):
                    //  (a) Sales-only query → effective Agent id(s)
                    //  (b) cross-module IOfficerAgentUserLookup → MO UserId(s)
                    var agentIds = await _queryRepository.GetComplaintAgentIdsAsync(newId);

                    var moUserIds = new List<int>();
                    if (agentIds.Count > 0)
                    {
                        var moChain = await _officerAgentUserLookup
                            .GetMarketingOfficerChainByAgentIdsAsync(agentIds, cancellationToken);
                        moUserIds = moChain
                            .Where(r => r.MoUserId.HasValue && r.MoUserId.Value > 0)
                            .Select(r => r.MoUserId!.Value)
                            .Distinct()
                            .ToList();
                    }

                    if (moUserIds.Count > 0)
                        _logger.LogInformation(
                            "Resolved {Count} agent-MO recipient(s) for 'New Complaint' InApp, Complaint {Id}: [{Users}]",
                            moUserIds.Count, newId, string.Join(",", moUserIds));
                    else
                        _logger.LogWarning(
                            "No agent-MO recipient resolved for 'New Complaint' InApp, Complaint {Id} — " +
                            "falling back to configured notification target.", newId);

                    var inAppCorrelationId = Guid.NewGuid();
                    var inAppEvent = new NotificationCreatedEvent
                    {
                        CorrelationId = inAppCorrelationId,
                        CreatedByName = _ipAddressService.GetUserName() ?? string.Empty,
                        UnitId        = unitId ?? 0,
                        ModuleName    = MiscEnumEntity.NotifModuleNewComplaint,
                        EventTypeId   = createEventType.Id,
                        Email         = string.Empty,
                        ccMail        = string.Empty,
                        Mobile        = string.Empty,
                        param1        = complaintNumber,
                        param2        = string.Empty,
                        param3        = DateTimeOffset.UtcNow,
                        param4        = string.Empty,
                        param5        = _ipAddressService.GetUserName() ?? string.Empty,
                        param6        = string.Empty,
                        param7        = string.Empty,
                        param8        = string.Empty,
                        param9        = string.Empty,
                        param10       = string.Empty,
                        // Dynamic recipient override (Agent's MO). Null/empty when nothing
                        // resolved → dispatcher falls back to the configured target.
                        OverrideTargetUserIds = moUserIds.Count > 0 ? moUserIds : null,
                        ModuleTransactionId = newId,
                        ModuleTypeName = MiscEnumEntity.ComplaintModuleTypeName
                    };
                    await _outboxEventPublisher.ScheduleAsync(inAppEvent, inAppCorrelationId, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish 'New Complaint' InApp notification for Complaint {Id}", newId);
            }

            // Publish workflow approval request via Outbox
            var correlationId = Guid.NewGuid();
            var payload = JsonSerializer.Serialize(new
            {
                Header = new
                {
                    Id = newId,
                    ComplaintNumber = complaintNumber,
                    CustomerId = request.CustomerId,
                    UnitId = unitId ?? 0,
                    StatusId = entity.StatusId
                },
                Lines = new List<object>()
            });

            var workflowEvent = new CreateApprovalRequestCommand
            {
                CorrelationId = correlationId,
                ModuleTypeName = MiscEnumEntity.ComplaintModuleTypeName,
                ModuleTransactionId = newId,
                Payload = payload
            };
            await _outboxEventPublisher.ScheduleAsync(workflowEvent, correlationId, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "COMPLAINT_CREATE",
                actionName: complaintNumber,
                details: $"Complaint '{complaintNumber}' created successfully with Id {newId}.",
                module: "Complaint");
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Complaint created successfully.",
                Data = newId
            };
        }
    }
}
