using AutoMapper;
using Contracts.Commands.Workflow;
using Contracts.Common;
using Contracts.Interfaces;
using MediatR;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Application.Common.Interfaces.IComplaintQCReview;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Application.Common.Interfaces.IOutbox;
using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using SalesManagement.Domain.Events;
using System.Text.Json;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Application.ComplaintQCReview.Commands.UpdateQCReview
{
    public class UpdateQCReviewCommandHandler : IRequestHandler<UpdateQCReviewCommand, ApiResponseDTO<int>>
    {
        private readonly IComplaintQCReviewCommandRepository _commandRepository;
        private readonly IComplaintQCReviewQueryRepository _queryRepository;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly ITimeZoneService _timeZoneService;
        private readonly IOutboxEventPublisher _outboxEventPublisher;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateQCReviewCommandHandler(
            IComplaintQCReviewCommandRepository commandRepository,
            IComplaintQCReviewQueryRepository queryRepository,
            IMiscMasterQueryRepository miscMasterQueryRepository,
            IIPAddressService ipAddressService,
            ITimeZoneService timeZoneService,
            IOutboxEventPublisher outboxEventPublisher,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _miscMasterQueryRepository = miscMasterQueryRepository;
            _ipAddressService = ipAddressService;
            _timeZoneService = timeZoneService;
            _outboxEventPublisher = outboxEventPublisher;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateQCReviewCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.ComplaintQCReview>(request);

            // Set QC Review audit fields
            var userId = _ipAddressService.GetUserId();
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
            var assignments = new List<ComplaintQCReviewAssignment>();
            if (request.Assignments != null && request.Assignments.Count > 0)
            {
                var pendingStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                    MiscEnumEntity.QCAssignmentStatus, MiscEnumEntity.AssignmentPending);

                assignments = request.Assignments.Select(a => new ComplaintQCReviewAssignment
                {
                    ComplaintQCReviewId = request.Id,
                    RoleId = a.RoleId,
                    ResponsiblePersonId = a.ResponsiblePersonId,
                    IsMandatory = a.IsMandatory,
                    AssignmentStatusId = pendingStatus?.Id ?? 0,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                }).ToList();
            }

            var result = await _commandRepository.UpdateAsync(entity, assignments);

            // Get ComplaintHeaderId from existing QC Review
            var existingReview = await _queryRepository.GetByIdAsync(request.Id);
            if (existingReview != null)
            {
                // Publish workflow approval request via Outbox
                var correlationId = Guid.NewGuid();
                var unitId = _ipAddressService.GetUnitId();
                var payload = JsonSerializer.Serialize(new
                {
                    Header = new
                    {
                        Id = existingReview.ComplaintHeaderId,
                        QCReviewId = request.Id,
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
                    ModuleTransactionId = existingReview.ComplaintHeaderId,
                    Payload = payload
                };
                await _outboxEventPublisher.ScheduleAsync(workflowEvent, correlationId, cancellationToken);
            }

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "COMPLAINT_QC_REVIEW_UPDATE",
                actionName: request.Id.ToString(),
                details: $"QC Review with Id {request.Id} updated successfully.",
                module: "ComplaintQCReview");
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "QC Review updated successfully.",
                Data = result
            };
        }
    }
}
