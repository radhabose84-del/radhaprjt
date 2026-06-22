using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using FinanceManagement.Application.Common.Interfaces;
using FinanceManagement.Application.Common.Interfaces.ICoaChangeRequest;
using FinanceManagement.Application.Common.Options;
using FinanceManagement.Domain.Entities;
using FinanceManagement.Domain.Events;
using MediatR;
using Microsoft.Extensions.Options;

namespace FinanceManagement.Application.CoaChangeRequest.Commands.ApproveCoaChangeImpact
{
    public class ApproveCoaChangeImpactCommandHandler : IRequestHandler<ApproveCoaChangeImpactCommand, ApiResponseDTO<bool>>
    {
        private readonly ICoaChangeRequestCommandRepository _commandRepository;
        private readonly IRoleUserLookup _roleUserLookup;
        private readonly IIPAddressService _ipAddressService;
        private readonly ITimeZoneService _timeZoneService;
        private readonly CoaUnfreezeOptions _options;
        private readonly IMediator _mediator;

        public ApproveCoaChangeImpactCommandHandler(
            ICoaChangeRequestCommandRepository commandRepository,
            IRoleUserLookup roleUserLookup,
            IIPAddressService ipAddressService,
            ITimeZoneService timeZoneService,
            IOptions<CoaUnfreezeOptions> options,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _roleUserLookup = roleUserLookup;
            _ipAddressService = ipAddressService;
            _timeZoneService = timeZoneService;
            _options = options.Value;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<bool>> Handle(ApproveCoaChangeImpactCommand request, CancellationToken cancellationToken)
        {
            var userId = _ipAddressService.GetUserId();
            var now = _timeZoneService.GetCurrentTime();

            // AC5 — the impact assessment must be approved by the CFO specifically.
            var isCfo = await _roleUserLookup.UserHasRoleAsync(userId, _options.CfoRoleId, cancellationToken);
            if (!isCfo)
                throw new ExceptionRules("Only the CFO can approve the impact assessment.");

            var entity = await _commandRepository.GetChangeRequestAsync(request.ChangeRequestId, cancellationToken)
                ?? throw new ExceptionRules("Change request not found.");

            if (entity.RequestStatus != CoaChangeRequestStatus.PendingImpactApproval)
                throw new ExceptionRules($"Change request is '{entity.RequestStatus}' — only a request pending impact approval can be approved.");

            entity.ImpactApprovedByUserId = userId;
            entity.ImpactApprovedOn = now;
            entity.RequestStatus = CoaChangeRequestStatus.ImpactApproved;
            await _commandRepository.SaveChangesAsync(cancellationToken);

            await _mediator.Publish(new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "COA_CHANGE_IMPACT_APPROVED",
                actionName: entity.Id.ToString(),
                details: $"CFO (user {userId}) approved the impact assessment for change request {entity.Id}.",
                module: "CoaChangeRequest"), cancellationToken);

            return new ApiResponseDTO<bool>
            {
                IsSuccess = true,
                Message = "Impact assessment approved. The change request can now be added to an unfreeze request.",
                Data = true
            };
        }
    }
}
