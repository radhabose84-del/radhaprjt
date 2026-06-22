using Contracts.Common;
using Contracts.Events.Coa;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using FinanceManagement.Application.Common.Interfaces;
using FinanceManagement.Application.Common.Interfaces.ICoaChangeRequest;
using FinanceManagement.Application.Common.Interfaces.ICoaFreeze;
using FinanceManagement.Application.Common.Interfaces.IOutbox;
using FinanceManagement.Application.Common.Options;
using FinanceManagement.Domain.Entities;
using FinanceManagement.Domain.Events;
using MediatR;
using Microsoft.Extensions.Options;

namespace FinanceManagement.Application.CoaChangeRequest.Commands.ApproveCoaUnfreeze
{
    public class ApproveCoaUnfreezeCommandHandler : IRequestHandler<ApproveCoaUnfreezeCommand, ApiResponseDTO<bool>>
    {
        // AC1 — exact message when one person tries to give both approvals.
        private const string DualApprovalMessage = "Dual approval required — approvers must be different users.";

        private readonly ICoaChangeRequestCommandRepository _commandRepository;
        private readonly ICoaFreezeCommandRepository _freezeCommandRepository;
        private readonly IRoleUserLookup _roleUserLookup;
        private readonly IOutboxEventPublisher _outboxEventPublisher;
        private readonly IIPAddressService _ipAddressService;
        private readonly ITimeZoneService _timeZoneService;
        private readonly CoaUnfreezeOptions _options;
        private readonly IMediator _mediator;

        public ApproveCoaUnfreezeCommandHandler(
            ICoaChangeRequestCommandRepository commandRepository,
            ICoaFreezeCommandRepository freezeCommandRepository,
            IRoleUserLookup roleUserLookup,
            IOutboxEventPublisher outboxEventPublisher,
            IIPAddressService ipAddressService,
            ITimeZoneService timeZoneService,
            IOptions<CoaUnfreezeOptions> options,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _freezeCommandRepository = freezeCommandRepository;
            _roleUserLookup = roleUserLookup;
            _outboxEventPublisher = outboxEventPublisher;
            _ipAddressService = ipAddressService;
            _timeZoneService = timeZoneService;
            _options = options.Value;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<bool>> Handle(ApproveCoaUnfreezeCommand request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");
            var userId = _ipAddressService.GetUserId();
            var now = _timeZoneService.GetCurrentTime();

            var req = await _commandRepository.GetUnfreezeRequestAsync(request.UnfreezeRequestId, cancellationToken)
                ?? throw new ExceptionRules("Unfreeze request not found.");

            if (req.RequestStatus != CoaUnfreezeRequestStatus.PendingApproval)
                throw new ExceptionRules($"Unfreeze request is '{req.RequestStatus}' — only a request pending approval can be approved.");

            var isCfo = await _roleUserLookup.UserHasRoleAsync(userId, _options.CfoRoleId, cancellationToken);
            var isSysAdmin = await _roleUserLookup.UserHasRoleAsync(userId, _options.SystemAdminRoleId, cancellationToken);
            if (!isCfo && !isSysAdmin)
                throw new ExceptionRules("You must hold the CFO or System Admin role to approve an unfreeze.");

            // Fill the slot the caller qualifies for, honouring the distinct-user rule (AC1).
            var filled = false;
            if (isCfo && req.CfoApproverUserId == null)
            {
                if (req.SysAdminApproverUserId == userId)
                    throw new ExceptionRules(DualApprovalMessage);
                req.CfoApproverUserId = userId;
                req.CfoApprovedOn = now;
                filled = true;
            }
            if (!filled && isSysAdmin && req.SysAdminApproverUserId == null)
            {
                if (req.CfoApproverUserId == userId)
                    throw new ExceptionRules(DualApprovalMessage);
                req.SysAdminApproverUserId = userId;
                req.SysAdminApprovedOn = now;
                filled = true;
            }
            if (!filled)
            {
                if (req.CfoApproverUserId == userId || req.SysAdminApproverUserId == userId)
                    throw new ExceptionRules("You have already approved this unfreeze request.");
                throw new ExceptionRules("This unfreeze request already has the approval for your role.");
            }

            var bothApproved = req.CfoApproverUserId.HasValue
                               && req.SysAdminApproverUserId.HasValue
                               && req.CfoApproverUserId.Value != req.SysAdminApproverUserId.Value;

            string message;
            if (bothApproved)
            {
                // AC2 — activate the time-boxed window; 08A enforces auto-re-freeze on expiry.
                var expiry = now.AddMinutes(req.WindowMinutes > 0 ? req.WindowMinutes : _options.DefaultWindowMinutes);
                req.RequestStatus = CoaUnfreezeRequestStatus.WindowOpen;
                req.WindowOpenedOn = now;
                req.WindowExpiry = expiry;

                // Drive 08A's freeze state open (IsFrozen = 0, UnfreezeWindowExpiry = expiry).
                await _freezeCommandRepository.OpenUnfreezeWindowAsync(companyId, expiry, cancellationToken);

                // AC2 alerts — recipients (CFO / FC / Internal Audit) pre-resolved; sent by BackgroundService.
                var recipients = await ResolveAlertRecipientsAsync(cancellationToken);
                var correlationId = Guid.NewGuid();
                await _outboxEventPublisher.ScheduleWithoutSaveAsync(new CoaUnfreezeAlertEvent
                {
                    CorrelationId = correlationId,
                    CompanyId = companyId,
                    UnfreezeRequestId = req.Id,
                    Reason = req.Reason,
                    CfoApproverUserId = req.CfoApproverUserId!.Value,
                    SysAdminApproverUserId = req.SysAdminApproverUserId!.Value,
                    WindowExpiry = expiry,
                    RecipientEmails = recipients
                }, correlationId, cancellationToken);

                message = "Dual approval complete — unfreeze window opened. Alerts sent to CFO, FC and Internal Audit.";
            }
            else
            {
                message = "Approval recorded. Awaiting the second distinct approver (CFO + System Admin).";
            }

            await _commandRepository.SaveChangesAsync(cancellationToken);

            await _mediator.Publish(new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: bothApproved ? "COA_UNFREEZE_WINDOW_OPENED" : "COA_UNFREEZE_APPROVAL_RECORDED",
                actionName: req.Id.ToString(),
                details: bothApproved
                    ? $"Unfreeze {req.Id} dual-approved (CFO {req.CfoApproverUserId}, SysAdmin {req.SysAdminApproverUserId}); window open until {req.WindowExpiry:u}."
                    : $"Unfreeze {req.Id}: approval recorded by user {userId}.",
                module: "CoaChangeRequest"), cancellationToken);

            return new ApiResponseDTO<bool> { IsSuccess = true, Message = message, Data = true };
        }

        private async Task<List<string>> ResolveAlertRecipientsAsync(CancellationToken ct)
        {
            var emails = new List<string>();
            foreach (var roleId in new[] { _options.CfoRoleId, _options.FcRoleId, _options.InternalAuditRoleId })
            {
                if (roleId <= 0) continue;
                emails.AddRange(await _roleUserLookup.GetEmailsByRoleAsync(roleId, ct));
            }
            return emails
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .Select(e => e.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}
