using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IPeriodStatusOverride;
using FinanceManagement.Application.Common.PeriodStatus;
using FinanceManagement.Domain.Events;
using MediatR;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Application.PeriodStatusOverride.Commands.RequestPeriodReversal
{
    public class RequestPeriodReversalCommandHandler : IRequestHandler<RequestPeriodReversalCommand, ApiResponseDTO<int>>
    {
        private readonly IPeriodStatusOverrideCommandRepository _commandRepository;
        private readonly IPeriodStatusOverrideQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly ITimeZoneService _timeZoneService;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public RequestPeriodReversalCommandHandler(
            IPeriodStatusOverrideCommandRepository commandRepository,
            IPeriodStatusOverrideQueryRepository queryRepository,
            IIPAddressService ipAddressService,
            ITimeZoneService timeZoneService,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
            _timeZoneService = timeZoneService;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(RequestPeriodReversalCommand request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId() ?? throw new ExceptionRules("No active company in session.");
            var userId    = _ipAddressService.GetUserId();
            var now       = _timeZoneService.GetCurrentTime();

            var snap = await _queryRepository.GetPeriodSnapshotAsync(request.PeriodId, cancellationToken)
                       ?? throw new ExceptionRules("Period not found.");

            if (snap.CompanyId != companyId)
                throw new ExceptionRules("Period not found for this company.");

            if (!PeriodStatusStateMachine.IsValidReversal(snap.StatusCode, request.TargetStatusCode))
                throw new ExceptionRules(
                    $"Reversal direction not allowed: {snap.StatusCode} -> {request.TargetStatusCode}. " +
                    "Allowed: HARDCLOSED -> SOFTCLOSED, SOFTCLOSED -> OPEN.");

            if (await _queryRepository.HasOpenOverrideAsync(request.PeriodId))
                throw new ExceptionRules("An override request is already in progress for this period.");

            var toStatusId  = await _queryRepository.GetMiscMasterIdByCodeAsync("FPS", request.TargetStatusCode!.ToUpperInvariant());
            var pendingId   = await _queryRepository.GetMiscMasterIdByCodeAsync("PSO", PeriodStatusConstants.OverridePending);
            if (toStatusId <= 0 || pendingId <= 0)
                throw new ExceptionRules("Status master rows (FPS / PSO) are not seeded.");

            var entity = _mapper.Map<Domain.Entities.PeriodStatusOverride>(request);
            entity.AccountingPeriodId = request.PeriodId;
            entity.CompanyId          = companyId;
            entity.FromStatusId       = snap.StatusId;
            entity.ToStatusId         = toStatusId;
            entity.RequestedBy        = userId;
            entity.RequestedAt        = now;
            entity.OverrideStatusId   = pendingId;
            entity.IsActive           = Status.Active;
            entity.IsDeleted          = IsDelete.NotDeleted;

            var newId = await _commandRepository.CreateAsync(entity, cancellationToken);

            await _mediator.Publish(new AuditLogsDomainEvent(
                "Create", "PERIOD_REVERSAL_REQUEST", newId.ToString(),
                $"PeriodStatusOverride {newId} requested for Period {request.PeriodId}: {snap.StatusCode} -> {request.TargetStatusCode}.",
                "PeriodStatusOverride"), cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Reversal request created. Awaiting CFO + SysAdmin approvals.",
                Data = newId
            };
        }
    }
}
