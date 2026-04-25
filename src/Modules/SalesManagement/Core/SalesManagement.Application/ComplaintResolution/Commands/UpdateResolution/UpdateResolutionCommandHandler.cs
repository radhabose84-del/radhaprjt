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

namespace SalesManagement.Application.ComplaintResolution.Commands.UpdateResolution
{
    public class UpdateResolutionCommandHandler : IRequestHandler<UpdateResolutionCommand, ApiResponseDTO<int>>
    {
        private readonly IComplaintResolutionCommandRepository _commandRepository;
        private readonly IComplaintResolutionQueryRepository _queryRepository;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly ITimeZoneService _timeZoneService;
        private readonly IOutboxEventPublisher _outboxEventPublisher;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateResolutionCommandHandler(
            IComplaintResolutionCommandRepository commandRepository,
            IComplaintResolutionQueryRepository queryRepository,
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

        public async Task<ApiResponseDTO<int>> Handle(UpdateResolutionCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.ComplaintResolution>(request);

            var userId = _ipAddressService.GetUserId();
            var systemTimeZoneId = _timeZoneService.GetSystemTimeZone();
            var currentTime = _timeZoneService.GetCurrentTime(systemTimeZoneId);

            // Auto-close: if the resolver has filled the type-specific "done" signal,
            // promote ClosureStatus to "Closed" automatically. Manual selection of
            // "Closed" is blocked by the validator, so this is the only legitimate path.
            //   • No Action       — always closeable (nothing pending)
            //   • Credit Note     — FinanceReference filled (operator typed it after Tally posted)
            //   • Replacement     — DispatchReference filled (operator typed it after dispatch)
            //   • Reprocess       — ActionDescription filled
            //   • Sales Return    — left to the SalesReturn-receipt hook (separate workstream)
            var resolutionType = await _miscMasterQueryRepository.GetByIdAsync(request.ResolutionTypeId);
            var resolutionTypeCode = resolutionType?.Code;

            bool autoCloseEligible = resolutionTypeCode switch
            {
                MiscEnumEntity.ResolutionNoAction    => true,
                MiscEnumEntity.ResolutionCreditNote  => !string.IsNullOrWhiteSpace(request.FinanceReference),
                MiscEnumEntity.ResolutionReplacement => !string.IsNullOrWhiteSpace(request.DispatchReference),
                MiscEnumEntity.ResolutionReprocess   => !string.IsNullOrWhiteSpace(request.ActionDescription),
                _                                    => false
            };

            if (autoCloseEligible)
            {
                var closedStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                    MiscEnumEntity.ClosureStatus, MiscEnumEntity.ClosureStatusClosed);
                if (closedStatus != null)
                {
                    entity.ClosureStatusId = closedStatus.Id;
                    entity.ClosedBy = userId;
                    entity.ClosedDate = currentTime;
                }
            }

            var resultId = await _commandRepository.UpdateAsync(entity);

            // Get ComplaintHeaderId from existing Resolution
            var existingResolution = await _queryRepository.GetByIdAsync(request.Id);
            if (existingResolution != null)
            {
                // Publish workflow approval request via Outbox
                var correlationId = Guid.NewGuid();
                var unitId = _ipAddressService.GetUnitId();
                var payload = JsonSerializer.Serialize(new
                {
                    Header = new
                    {
                        Id = existingResolution.ComplaintHeaderId,
                        ResolutionId = request.Id,
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
                    ModuleTransactionId = existingResolution.ComplaintHeaderId,
                    Payload = payload
                };
                await _outboxEventPublisher.ScheduleAsync(workflowEvent, correlationId, cancellationToken);
            }

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "COMPLAINT_RESOLUTION_UPDATE",
                actionName: resultId.ToString(),
                details: $"Resolution updated with Id {resultId}.",
                module: "ComplaintResolution");
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Resolution updated successfully.",
                Data = resultId
            };
        }
    }
}
