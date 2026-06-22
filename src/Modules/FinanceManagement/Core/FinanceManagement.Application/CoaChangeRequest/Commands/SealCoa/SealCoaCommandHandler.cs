using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using FinanceManagement.Application.Common.Interfaces;
using FinanceManagement.Application.Common.Interfaces.ICoaFreeze;
using FinanceManagement.Application.Common.Options;
using FinanceManagement.Domain.Events;
using MediatR;
using Microsoft.Extensions.Options;

namespace FinanceManagement.Application.CoaChangeRequest.Commands.SealCoa
{
    public class SealCoaCommandHandler : IRequestHandler<SealCoaCommand, ApiResponseDTO<bool>>
    {
        private readonly ICoaFreezeCommandRepository _freezeCommandRepository;
        private readonly IRoleUserLookup _roleUserLookup;
        private readonly IIPAddressService _ipAddressService;
        private readonly ITimeZoneService _timeZoneService;
        private readonly CoaUnfreezeOptions _options;
        private readonly IMediator _mediator;

        public SealCoaCommandHandler(
            ICoaFreezeCommandRepository freezeCommandRepository,
            IRoleUserLookup roleUserLookup,
            IIPAddressService ipAddressService,
            ITimeZoneService timeZoneService,
            IOptions<CoaUnfreezeOptions> options,
            IMediator mediator)
        {
            _freezeCommandRepository = freezeCommandRepository;
            _roleUserLookup = roleUserLookup;
            _ipAddressService = ipAddressService;
            _timeZoneService = timeZoneService;
            _options = options.Value;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<bool>> Handle(SealCoaCommand request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");
            var userId = _ipAddressService.GetUserId();
            var now = _timeZoneService.GetCurrentTime();

            var isCfo = await _roleUserLookup.UserHasRoleAsync(userId, _options.CfoRoleId, cancellationToken);
            if (!isCfo)
                throw new ExceptionRules("Only the CFO can seal the Chart of Accounts.");

            await _freezeCommandRepository.FreezeAsync(companyId, userId, now, cancellationToken);

            await _mediator.Publish(new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "COA_SEALED",
                actionName: companyId.ToString(),
                details: $"Chart of Accounts sealed by CFO (user {userId}).",
                module: "CoaChangeRequest"), cancellationToken);

            return new ApiResponseDTO<bool> { IsSuccess = true, Message = "Chart of Accounts sealed.", Data = true };
        }
    }
}
