using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IPeriodStatusOverride;
using FinanceManagement.Application.Common.PeriodStatus;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.PeriodStatusOverride.Commands.RejectPeriodReversal
{
    public class RejectPeriodReversalCommandHandler : IRequestHandler<RejectPeriodReversalCommand, ApiResponseDTO<int>>
    {
        private readonly IPeriodStatusOverrideCommandRepository _commandRepository;
        private readonly IPeriodStatusOverrideQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly ITimeZoneService _timeZoneService;
        private readonly IMediator _mediator;

        public RejectPeriodReversalCommandHandler(
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

        public async Task<ApiResponseDTO<int>> Handle(RejectPeriodReversalCommand request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId() ?? throw new ExceptionRules("No active company in session.");
            var userId    = _ipAddressService.GetUserId();
            var now       = _timeZoneService.GetCurrentTime();

            var ovr = await _queryRepository.GetByIdAsync(request.OverrideId)
                       ?? throw new ExceptionRules("Override request not found.");

            if (ovr.CompanyId != companyId)
                throw new ExceptionRules("Override request not found for this company.");

            if (!string.Equals(ovr.OverrideStatusCode, PeriodStatusConstants.OverridePending, StringComparison.OrdinalIgnoreCase))
                throw new ExceptionRules($"Override is already finalised (status: {ovr.OverrideStatusCode}).");

            if (ovr.RequestedBy == userId)
                throw new ExceptionRules("Requester cannot self-reject.");

            var rejectedId = await _queryRepository.GetMiscMasterIdByCodeAsync("PSO", PeriodStatusConstants.OverrideRejected);
            if (rejectedId <= 0)
                throw new ExceptionRules("MiscMaster row PSO/REJECTED is not seeded.");

            var entity = new Domain.Entities.PeriodStatusOverride
            {
                Id                  = ovr.Id,
                AccountingPeriodId   = ovr.AccountingPeriodId,
                CompanyId           = ovr.CompanyId,
                FromStatusId        = ovr.FromStatusId,
                ToStatusId          = ovr.ToStatusId,
                RequestedBy         = ovr.RequestedBy,
                RequestedAt         = ovr.RequestedAt,
                RequestedReason     = ovr.RequestedReason,
                CfoApproverId       = ovr.CfoApproverId,
                CfoApprovedAt       = ovr.CfoApprovedAt,
                SysAdminApproverId  = ovr.SysAdminApproverId,
                SysAdminApprovedAt  = ovr.SysAdminApprovedAt,
                OverrideStatusId    = rejectedId,
                AppliedAt           = ovr.AppliedAt,
                RejectionReason     = request.RejectionReason
            };

            await _commandRepository.UpdateAsync(entity, cancellationToken);

            await _mediator.Publish(new AuditLogsDomainEvent(
                "Update", "PERIOD_REVERSAL_REJECTED", entity.Id.ToString(),
                $"Override {entity.Id} rejected: {request.RejectionReason}",
                "PeriodStatusOverride"), cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Override rejected.",
                Data = entity.Id
            };
        }
    }
}
