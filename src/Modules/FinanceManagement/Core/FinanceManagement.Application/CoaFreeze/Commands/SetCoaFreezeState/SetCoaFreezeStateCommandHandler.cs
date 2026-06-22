using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces;
using FinanceManagement.Application.Common.Interfaces.ICoaFreeze;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.CoaFreeze.Commands.SetCoaFreezeState
{
    public class SetCoaFreezeStateCommandHandler : IRequestHandler<SetCoaFreezeStateCommand, ApiResponseDTO<bool>>
    {
        private readonly ICoaFreezeCommandRepository _commandRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly ITimeZoneService _timeZoneService;
        private readonly IMediator _mediator;

        public SetCoaFreezeStateCommandHandler(
            ICoaFreezeCommandRepository commandRepository,
            IIPAddressService ipAddressService,
            ITimeZoneService timeZoneService,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _ipAddressService = ipAddressService;
            _timeZoneService = timeZoneService;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<bool>> Handle(SetCoaFreezeStateCommand request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");
            var userId = _ipAddressService.GetUserId();
            var now = _timeZoneService.GetCurrentTime();

            // US-GL02-08B (gap G1): unfreeze is now governed by the dual-approval workflow. This TEST/ADMIN
            // hook may only seal — opening an unfreeze window directly would bypass CFO + System Admin
            // dual approval, defeating the whole control. Sealing here is superseded by SealCoaCommand.
            if (!request.IsFrozen)
                throw new ExceptionRules(
                    "Direct unfreeze is disabled. Raise a change request and obtain dual approval (CFO + System Admin) via the COA unfreeze workflow (US-GL02-08B).");

            await _commandRepository.FreezeAsync(companyId, userId, now, cancellationToken);
            var message = "Chart of Accounts frozen.";

            await _mediator.Publish(new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: request.IsFrozen ? "COA_FREEZE" : "COA_UNFREEZE_WINDOW_OPEN",
                actionName: companyId.ToString(),
                details: message,
                module: "CoaFreeze"), cancellationToken);

            return new ApiResponseDTO<bool> { IsSuccess = true, Message = message, Data = true };
        }
    }
}
