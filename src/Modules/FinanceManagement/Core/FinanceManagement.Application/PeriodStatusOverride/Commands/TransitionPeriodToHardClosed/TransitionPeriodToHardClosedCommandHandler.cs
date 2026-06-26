using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IPeriodStatusOverride;
using FinanceManagement.Application.Common.PeriodStatus;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.PeriodStatusOverride.Commands.TransitionPeriodToHardClosed
{
    public class TransitionPeriodToHardClosedCommandHandler : IRequestHandler<TransitionPeriodToHardClosedCommand, ApiResponseDTO<int>>
    {
        private readonly IPeriodStatusOverrideCommandRepository _commandRepository;
        private readonly IPeriodStatusOverrideQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly ITimeZoneService _timeZoneService;
        private readonly IMediator _mediator;

        public TransitionPeriodToHardClosedCommandHandler(
            IPeriodStatusOverrideCommandRepository commandRepository,
            IPeriodStatusOverrideQueryRepository queryRepository,
            IIPAddressService ipAddressService,
            ITimeZoneService timeZoneService,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
            _timeZoneService = timeZoneService;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<int>> Handle(TransitionPeriodToHardClosedCommand request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId() ?? throw new ExceptionRules("No active company in session.");
            var userId    = _ipAddressService.GetUserId();
            var now       = _timeZoneService.GetCurrentTime();

            var snap = await _queryRepository.GetPeriodSnapshotAsync(request.PeriodId, cancellationToken)
                       ?? throw new ExceptionRules("Period not found.");

            if (snap.CompanyId != companyId)
                throw new ExceptionRules("Period not found for this company.");

            PeriodStatusStateMachine.AssertForwardTransition(snap.StatusCode, PeriodStatusConstants.HardClosed);

            var newStatusId = await _queryRepository.GetMiscMasterIdByCodeAsync("FPS", PeriodStatusConstants.HardClosed);
            if (newStatusId <= 0)
                throw new ExceptionRules("MiscMaster row for FPS/HARDCLOSED is not seeded.");

            var applied = await _commandRepository.ApplyPeriodStatusChangeAsync(
                request.PeriodId, newStatusId, userId, now, null, null, cancellationToken);
            if (!applied)
                throw new ExceptionRules("Failed to apply status change.");

            await _mediator.Publish(new PeriodStatusChangedDomainEvent(
                AccountingPeriodId: request.PeriodId,
                CompanyId:         companyId,
                FinancialYearId:   snap.FinancialYearId,
                FromStatusId:      snap.StatusId,
                FromStatusCode:    snap.StatusCode,
                ToStatusId:        newStatusId,
                ToStatusCode:      PeriodStatusConstants.HardClosed,
                ChangedBy:         userId,
                ChangedAt:         now,
                IsReversal:        false,
                OverrideId:        null), cancellationToken);

            await _mediator.Publish(new AuditLogsDomainEvent(
                "Update", "PERIOD_HARD_CLOSE", request.PeriodId.ToString(),
                $"FinancialPeriod {request.PeriodId} transitioned {snap.StatusCode} -> HARDCLOSED.",
                "FinancialPeriodMaster"), cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Period hard-closed successfully.",
                Data = request.PeriodId
            };
        }
    }
}
