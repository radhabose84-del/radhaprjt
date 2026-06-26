using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IPeriodStatusOverride;
using FinanceManagement.Application.Common.PeriodStatus;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.PeriodStatusOverride.Commands.ApprovePeriodReversal
{
    public class ApprovePeriodReversalCommandHandler : IRequestHandler<ApprovePeriodReversalCommand, ApiResponseDTO<int>>
    {
        private readonly IPeriodStatusOverrideCommandRepository _commandRepository;
        private readonly IPeriodStatusOverrideQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly ITimeZoneService _timeZoneService;
        private readonly IMediator _mediator;

        public ApprovePeriodReversalCommandHandler(
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

        public async Task<ApiResponseDTO<int>> Handle(ApprovePeriodReversalCommand request, CancellationToken cancellationToken)
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
                throw new ExceptionRules("Requester cannot self-approve.");

            var role = (request.Role ?? string.Empty).Trim();
            var isCfo      = string.Equals(role, "CFO",      StringComparison.OrdinalIgnoreCase);
            var isSysAdmin = string.Equals(role, "SysAdmin", StringComparison.OrdinalIgnoreCase);
            if (!isCfo && !isSysAdmin)
                throw new ExceptionRules("Approver role must be 'CFO' or 'SysAdmin'.");

            if (isCfo && ovr.CfoApproverId.HasValue)
                throw new ExceptionRules("CFO approval already recorded.");
            if (isSysAdmin && ovr.SysAdminApproverId.HasValue)
                throw new ExceptionRules("SysAdmin approval already recorded.");

            // Apply approval
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
                CfoApproverId       = isCfo      ? userId : ovr.CfoApproverId,
                CfoApprovedAt       = isCfo      ? now    : ovr.CfoApprovedAt,
                SysAdminApproverId  = isSysAdmin ? userId : ovr.SysAdminApproverId,
                SysAdminApprovedAt  = isSysAdmin ? now    : ovr.SysAdminApprovedAt,
                OverrideStatusId    = ovr.OverrideStatusId,
                AppliedAt           = ovr.AppliedAt,
                RejectionReason     = ovr.RejectionReason
            };

            var bothApproved = entity.CfoApproverId.HasValue && entity.SysAdminApproverId.HasValue;
            int? finalOverrideStatusForApply = null;
            int? overrideIdForApply           = null;
            string? toCode                    = null;

            if (bothApproved)
            {
                // Auto-apply: flip period status + mark override APPLIED in one transaction
                var appliedStatusId = await _queryRepository.GetMiscMasterIdByCodeAsync("PSO", PeriodStatusConstants.OverrideApplied);
                if (appliedStatusId <= 0) throw new ExceptionRules("MiscMaster row PSO/APPLIED is not seeded.");

                finalOverrideStatusForApply = appliedStatusId;
                overrideIdForApply          = entity.Id;
                entity.OverrideStatusId     = appliedStatusId;
                entity.AppliedAt            = now;
                toCode                      = ovr.ToStatusCode;
            }

            await _commandRepository.UpdateAsync(entity, cancellationToken);

            if (bothApproved)
            {
                var ok = await _commandRepository.ApplyPeriodStatusChangeAsync(
                    entity.AccountingPeriodId, entity.ToStatusId, userId, now,
                    overrideIdToMarkApplied:        overrideIdForApply,
                    appliedStatusIdForOverride:     finalOverrideStatusForApply,
                    cancellationToken);
                if (!ok)
                    throw new ExceptionRules("Failed to apply period status — concurrent modification.");

                var snap = await _queryRepository.GetPeriodSnapshotAsync(entity.AccountingPeriodId, cancellationToken);

                await _mediator.Publish(new PeriodStatusChangedDomainEvent(
                    AccountingPeriodId: entity.AccountingPeriodId,
                    CompanyId:         entity.CompanyId,
                    FinancialYearId:   snap?.FinancialYearId ?? 0,
                    FromStatusId:      entity.FromStatusId,
                    FromStatusCode:    ovr.FromStatusCode,
                    ToStatusId:        entity.ToStatusId,
                    ToStatusCode:      toCode,
                    ChangedBy:         userId,
                    ChangedAt:         now,
                    IsReversal:        true,
                    OverrideId:        entity.Id), cancellationToken);

                await _mediator.Publish(new AuditLogsDomainEvent(
                    "Update", "PERIOD_REVERSAL_APPLIED", entity.Id.ToString(),
                    $"Override {entity.Id} applied: Period {entity.AccountingPeriodId} reverted {ovr.FromStatusCode} -> {toCode}.",
                    "PeriodStatusOverride"), cancellationToken);
            }
            else
            {
                await _mediator.Publish(new AuditLogsDomainEvent(
                    "Update", "PERIOD_REVERSAL_APPROVED", entity.Id.ToString(),
                    $"Override {entity.Id} partially approved by {role}.",
                    "PeriodStatusOverride"), cancellationToken);
            }

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = bothApproved
                    ? "Override approved by both CFO and SysAdmin; period status flipped."
                    : $"Approval recorded ({role}). Awaiting the other approver.",
                Data = entity.Id
            };
        }
    }
}
