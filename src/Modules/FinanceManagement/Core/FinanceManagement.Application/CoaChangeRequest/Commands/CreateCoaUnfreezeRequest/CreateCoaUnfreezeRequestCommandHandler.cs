using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces;
using FinanceManagement.Application.Common.Interfaces.ICoaChangeRequest;
using FinanceManagement.Application.Common.Options;
using FinanceManagement.Domain.Entities;
using FinanceManagement.Domain.Events;
using MediatR;
using Microsoft.Extensions.Options;

namespace FinanceManagement.Application.CoaChangeRequest.Commands.CreateCoaUnfreezeRequest
{
    public class CreateCoaUnfreezeRequestCommandHandler : IRequestHandler<CreateCoaUnfreezeRequestCommand, ApiResponseDTO<int>>
    {
        private readonly ICoaChangeRequestCommandRepository _commandRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly ITimeZoneService _timeZoneService;
        private readonly CoaUnfreezeOptions _options;
        private readonly IMediator _mediator;

        public CreateCoaUnfreezeRequestCommandHandler(
            ICoaChangeRequestCommandRepository commandRepository,
            IIPAddressService ipAddressService,
            ITimeZoneService timeZoneService,
            IOptions<CoaUnfreezeOptions> options,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _ipAddressService = ipAddressService;
            _timeZoneService = timeZoneService;
            _options = options.Value;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateCoaUnfreezeRequestCommand request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");
            var userId = _ipAddressService.GetUserId();
            var now = _timeZoneService.GetCurrentTime();

            // AC5 — only impact-approved change requests (for this company) may be batched.
            var changeRequests = await _commandRepository.GetImpactApprovedChangeRequestsAsync(
                request.ChangeRequestIds, companyId, cancellationToken);

            if (changeRequests.Count == 0)
                throw new ExceptionRules("No impact-approved change requests found to attach. A CFO-approved impact assessment is required first.");

            var windowMinutes = request.WindowMinutes is > 0 ? request.WindowMinutes!.Value : _options.DefaultWindowMinutes;

            var unfreeze = new CoaUnfreezeRequest
            {
                CompanyId = companyId,
                Reason = request.Reason,
                RequestStatus = CoaUnfreezeRequestStatus.PendingApproval,
                WindowMinutes = windowMinutes,
                RequestedByUserId = userId,
                RequestedOn = now
            };

            await _commandRepository.AddUnfreezeRequestWithoutSaveAsync(unfreeze, cancellationToken);
            await _commandRepository.SaveChangesAsync(cancellationToken);

            // Attach the impact-approved change requests to this window.
            foreach (var cr in changeRequests)
                cr.UnfreezeRequestId = unfreeze.Id;
            await _commandRepository.SaveChangesAsync(cancellationToken);

            await _mediator.Publish(new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "COA_UNFREEZE_REQUEST_CREATE",
                actionName: unfreeze.Id.ToString(),
                details: $"Unfreeze request {unfreeze.Id} raised for {changeRequests.Count} change request(s); pending CFO + System Admin dual approval.",
                module: "CoaChangeRequest"), cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = $"Unfreeze request raised for {changeRequests.Count} change request(s). Awaiting dual approval (CFO + System Admin).",
                Data = unfreeze.Id
            };
        }
    }
}
