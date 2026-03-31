using AutoMapper;
using Contracts.Commands.Workflow;
using Contracts.Common;
using Contracts.Interfaces;
using MediatR;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Application.Common.Interfaces.IComplaintResolution;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Application.Common.Interfaces.IOutbox;
using SalesManagement.Domain.Common;
using SalesManagement.Domain.Events;
using System.Text.Json;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Application.ComplaintResolution.Commands.SubmitResolution
{
    public class SubmitResolutionCommandHandler : IRequestHandler<SubmitResolutionCommand, ApiResponseDTO<int>>
    {
        private readonly IComplaintResolutionCommandRepository _commandRepository;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly ITimeZoneService _timeZoneService;
        private readonly IOutboxEventPublisher _outboxEventPublisher;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public SubmitResolutionCommandHandler(
            IComplaintResolutionCommandRepository commandRepository,
            IMiscMasterQueryRepository miscMasterQueryRepository,
            IIPAddressService ipAddressService,
            ITimeZoneService timeZoneService,
            IOutboxEventPublisher outboxEventPublisher,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _miscMasterQueryRepository = miscMasterQueryRepository;
            _ipAddressService = ipAddressService;
            _timeZoneService = timeZoneService;
            _outboxEventPublisher = outboxEventPublisher;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(SubmitResolutionCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.ComplaintResolution>(request);

            // Set resolved audit fields
            var userId = _ipAddressService.GetUserId();
            var systemTimeZoneId = _timeZoneService.GetSystemTimeZone();
            var currentTime = _timeZoneService.GetCurrentTime(systemTimeZoneId);

            entity.ResolvedBy = userId;
            entity.ResolvedDate = currentTime;

            // Auto-set ClosureStatus to "Open" if not provided
            if (!request.ClosureStatusId.HasValue || request.ClosureStatusId.Value == 0)
            {
                var openStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                    MiscEnumEntity.ClosureStatus, MiscEnumEntity.ClosureStatusOpen);
                entity.ClosureStatusId = openStatus?.Id;
            }

            // If closure status is "Closed", set closed audit fields
            if (request.ClosureStatusId.HasValue && request.ClosureStatusId.Value > 0)
            {
                var closedStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                    MiscEnumEntity.ClosureStatus, MiscEnumEntity.ClosureStatusClosed);
                if (closedStatus != null && request.ClosureStatusId.Value == closedStatus.Id)
                {
                    entity.ClosedBy = userId;
                    entity.ClosedDate = currentTime;
                }
            }

            var newId = await _commandRepository.CreateAsync(entity);

            // Publish workflow approval request via Outbox
            var correlationId = Guid.NewGuid();
            var unitId = _ipAddressService.GetUnitId();
            var payload = JsonSerializer.Serialize(new
            {
                Header = new
                {
                    Id = request.ComplaintHeaderId,
                    ResolutionId = newId,
                    ResolutionTypeId = request.ResolutionTypeId,
                    ClosureStatusId = request.ClosureStatusId,
                    CreditAmount = request.CreditAmount,
                    UnitId = unitId ?? 0
                },
                Lines = new List<object>()
            });

            var workflowEvent = new CreateApprovalRequestCommand
            {
                CorrelationId = correlationId,
                ModuleTypeName = MiscEnumEntity.ComplaintResolutionModuleTypeName,
                ModuleTransactionId = request.ComplaintHeaderId,
                Payload = payload
            };
            await _outboxEventPublisher.ScheduleAsync(workflowEvent, correlationId, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "COMPLAINT_RESOLUTION_SUBMIT",
                actionName: newId.ToString(),
                details: $"Resolution submitted for Complaint HeaderId {request.ComplaintHeaderId} with Id {newId}.",
                module: "ComplaintResolution");
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Resolution submitted successfully.",
                Data = newId
            };
        }
    }
}
