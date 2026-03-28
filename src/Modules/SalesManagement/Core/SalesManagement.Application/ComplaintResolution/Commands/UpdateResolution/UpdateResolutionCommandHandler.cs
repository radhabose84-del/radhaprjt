using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using MediatR;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Application.Common.Interfaces.IComplaintResolution;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Domain.Common;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.ComplaintResolution.Commands.UpdateResolution
{
    public class UpdateResolutionCommandHandler : IRequestHandler<UpdateResolutionCommand, ApiResponseDTO<int>>
    {
        private readonly IComplaintResolutionCommandRepository _commandRepository;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly ITimeZoneService _timeZoneService;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateResolutionCommandHandler(
            IComplaintResolutionCommandRepository commandRepository,
            IMiscMasterQueryRepository miscMasterQueryRepository,
            IIPAddressService ipAddressService,
            ITimeZoneService timeZoneService,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _miscMasterQueryRepository = miscMasterQueryRepository;
            _ipAddressService = ipAddressService;
            _timeZoneService = timeZoneService;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateResolutionCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.ComplaintResolution>(request);

            var userId = _ipAddressService.GetUserId();
            var systemTimeZoneId = _timeZoneService.GetSystemTimeZone();
            var currentTime = _timeZoneService.GetCurrentTime(systemTimeZoneId);

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

            var resultId = await _commandRepository.UpdateAsync(entity);

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
