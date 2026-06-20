using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IGlAccountMaster;
using MediatR;

namespace FinanceManagement.Application.GlAccountMaster.Commands.RecordGlAccountRecent
{
    // No audit event here — this fires on every account selection (high-frequency, low value);
    // the upsert itself carries the timestamp/usage.
    public class RecordGlAccountRecentCommandHandler : IRequestHandler<RecordGlAccountRecentCommand, ApiResponseDTO<bool>>
    {
        private readonly IGlAccountUserPrefStore _prefStore;
        private readonly IIPAddressService _ipAddressService;

        public RecordGlAccountRecentCommandHandler(
            IGlAccountUserPrefStore prefStore,
            IIPAddressService ipAddressService)
        {
            _prefStore = prefStore;
            _ipAddressService = ipAddressService;
        }

        public async Task<ApiResponseDTO<bool>> Handle(RecordGlAccountRecentCommand request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");
            var userId = _ipAddressService.GetUserId();

            await _prefStore.RecordRecentAsync(userId, companyId, request.GlAccountMasterId, cancellationToken);

            return new ApiResponseDTO<bool> { IsSuccess = true, Message = "Recorded.", Data = true };
        }
    }
}
