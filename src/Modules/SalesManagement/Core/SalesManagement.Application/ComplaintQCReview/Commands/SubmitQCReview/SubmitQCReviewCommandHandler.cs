using AutoMapper;
using Contracts.Commands.Workflow;
using Contracts.Common;
using Contracts.Events.Notifications;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Application.Common.Interfaces.IComplaintQCReview;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Application.Common.Interfaces.IOutbox;
using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using SalesManagement.Domain.Events;
using System.Text.Json;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Application.ComplaintQCReview.Commands.SubmitQCReview
{
    public class SubmitQCReviewCommandHandler : IRequestHandler<SubmitQCReviewCommand, ApiResponseDTO<int>>
    {
        private readonly IComplaintQCReviewCommandRepository _commandRepository;
        private readonly IComplaintQCReviewQueryRepository _queryRepository;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly ITimeZoneService _timeZoneService;
        private readonly IOutboxEventPublisher _outboxEventPublisher;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly ILogger<SubmitQCReviewCommandHandler> _logger;
        private readonly IAppDataMiscMasterLookup _appDataMiscLookup;

        public SubmitQCReviewCommandHandler(
            IComplaintQCReviewCommandRepository commandRepository,
            IComplaintQCReviewQueryRepository queryRepository,
            IMiscMasterQueryRepository miscMasterQueryRepository,
            IIPAddressService ipAddressService,
            ITimeZoneService timeZoneService,
            IOutboxEventPublisher outboxEventPublisher,
            IMediator mediator,
            IMapper mapper,
            ILogger<SubmitQCReviewCommandHandler> logger,
            IAppDataMiscMasterLookup appDataMiscLookup)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _miscMasterQueryRepository = miscMasterQueryRepository;
            _ipAddressService = ipAddressService;
            _timeZoneService = timeZoneService;
            _outboxEventPublisher = outboxEventPublisher;
            _mediator = mediator;
            _mapper = mapper;
            _logger = logger;
            _appDataMiscLookup = appDataMiscLookup;
        }

        public async Task<ApiResponseDTO<int>> Handle(SubmitQCReviewCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.ComplaintQCReview>(request);

            // Set QC Review audit fields
            var userId = _ipAddressService.GetUserId();
            var userName = _ipAddressService.GetUserName();
            var systemTimeZoneId = _timeZoneService.GetSystemTimeZone();
            var currentTime = _timeZoneService.GetCurrentTime(systemTimeZoneId);

            entity.ReviewedBy = userId;
            entity.ReviewedDate = currentTime;

            // Set decision timestamp if a decision was made
            if (request.ComplaintStatusId.HasValue && request.ComplaintStatusId.Value > 0)
            {
                entity.DecisionTimestamp = currentTime;
            }

            // Map assignments
            if (request.Assignments != null && request.Assignments.Count > 0)
            {
                // Resolve default "Pending" status for assignments
                var pendingStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                    MiscEnumEntity.QCAssignmentStatus, MiscEnumEntity.AssignmentPending);

                entity.Assignments = request.Assignments.Select(a => new ComplaintQCReviewAssignment
                {
                    RoleId = a.RoleId,
                    ResponsiblePersonId = a.ResponsiblePersonId,
                    IsMandatory = a.IsMandatory,
                    AssignmentStatusId = pendingStatus?.Id ?? 0,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                }).ToList();
            }

            var newId = await _commandRepository.CreateAsync(entity);

            var unitId = _ipAddressService.GetUnitId();

            // ------------------- Event 3 — InApp notification: QC Review submitted → all QC dept users -------------------
            // ModuleName='QC Review Submitted' matches NotificationConfig 33. EventTypeId resolved at
            // runtime. Dispatcher resolves recipients via TargetTypeId 2083 (COMPLAINT_QC_REVIEWER_USER)
            // → users in QC dept.
            try
            {
                var createEventType = await _appDataMiscLookup.GetMiscMasterByNameAsync(
                    MiscEnumEntity.NotifEventTypeMiscType, MiscEnumEntity.NotifEventTypeCreate);

                if (createEventType == null)
                {
                    _logger.LogWarning(
                        "MiscMaster EventType='{Code}' not found — skipping 'QC Review Submitted' InApp for QC Review {Id}",
                        MiscEnumEntity.NotifEventTypeCreate, newId);
                }
                else
                {
                    var inAppCorrelationId = Guid.NewGuid();
                    var inAppEvent = new NotificationCreatedEvent
                    {
                        CorrelationId = inAppCorrelationId,
                        CreatedByName = userName ?? string.Empty,
                        UnitId        = unitId ?? 0,
                        ModuleName    = MiscEnumEntity.NotifModuleQcReviewSubmitted,
                        EventTypeId   = createEventType.Id,
                        Email         = string.Empty,
                        ccMail        = string.Empty,
                        Mobile        = string.Empty,
                        param1        = request.ComplaintHeaderId.ToString(),
                        param2        = string.Empty,
                        param3        = currentTime,
                        param4        = request.SeverityId?.ToString() ?? string.Empty,
                        param5        = userName ?? string.Empty,
                        param6        = string.Empty,
                        param7        = string.Empty,
                        param8        = string.Empty,
                        param9        = string.Empty,
                        param10       = string.Empty,
                        ModuleTransactionId = newId,
                        ModuleTypeName = MiscEnumEntity.ComplaintQCReviewModuleTypeName
                    };
                    await _outboxEventPublisher.ScheduleAsync(inAppEvent, inAppCorrelationId, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish 'QC Review Submitted' InApp notification for QC Review {Id}", newId);
            }

            // Publish workflow approval request via Outbox
            var correlationId = Guid.NewGuid();
            var payload = JsonSerializer.Serialize(new
            {
                Header = new
                {
                    Id = request.ComplaintHeaderId,
                    QCReviewId = newId,
                    QCDecision = request.ComplaintStatusId,
                    SeverityId = request.SeverityId,
                    CompensationStructureId = request.CompensationStructureId,
                    UnitId = unitId ?? 0
                },
                Lines = new List<object>()
            });

            var workflowEvent = new CreateApprovalRequestCommand
            {
                CorrelationId = correlationId,
                ModuleTypeName = MiscEnumEntity.ComplaintQCReviewModuleTypeName,
                ModuleTransactionId = request.ComplaintHeaderId,
                Payload = payload
            };
            await _outboxEventPublisher.ScheduleAsync(workflowEvent, correlationId, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "COMPLAINT_QC_REVIEW_SUBMIT",
                actionName: newId.ToString(),
                details: $"QC Review submitted for Complaint HeaderId {request.ComplaintHeaderId} with Id {newId}.",
                module: "ComplaintQCReview");
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "QC Review submitted successfully.",
                Data = newId
            };
        }
    }
}
