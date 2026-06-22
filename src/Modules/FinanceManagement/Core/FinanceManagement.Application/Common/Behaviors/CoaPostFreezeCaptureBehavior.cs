using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces;
using FinanceManagement.Application.Common.Interfaces.ICoaChangeRequest;
using MediatR;

namespace FinanceManagement.Application.Common.Behaviors
{
    // US-GL02-08B (gap G2) — auto-capture. After a COA-structural write SUCCEEDS (which, while frozen, can
    // only happen inside an open unfreeze window), this links the edit to a pending change request in that
    // window: marks it Committed + Post-Freeze, stamps the committer and the account's LastPostFreezeChangeOn
    // (AC3). Best-effort — never breaks the real write. Linkage is heuristic (see TryCapturePostFreezeChangeAsync).
    public class CoaPostFreezeCaptureBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly ICoaChangeRequestCommandRepository _commandRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly ITimeZoneService _timeZoneService;

        public CoaPostFreezeCaptureBehavior(
            ICoaChangeRequestCommandRepository commandRepository,
            IIPAddressService ipAddressService,
            ITimeZoneService timeZoneService)
        {
            _commandRepository = commandRepository;
            _ipAddressService = ipAddressService;
            _timeZoneService = timeZoneService;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            // Run the actual write first; only capture on success (a freeze violation would throw before this).
            var response = await next();

            try
            {
                if (IsCoaStructuralChange(request, out var isAccount, out var isGroup))
                {
                    var companyId = _ipAddressService.GetCompanyId() ?? 0;
                    if (companyId > 0)
                    {
                        var userId = _ipAddressService.GetUserId();
                        var now = _timeZoneService.GetCurrentTime();
                        var targetId = ExtractId(request);
                        int? accountId = isAccount ? targetId : null;
                        int? groupId = isGroup ? targetId : null;

                        await _commandRepository.TryCapturePostFreezeChangeAsync(
                            companyId, accountId, groupId, userId, now, cancellationToken);
                    }
                }
            }
            catch
            {
                // Capture is best-effort — never mask or fail the underlying COA change.
            }

            return response;
        }

        private static bool IsCoaStructuralChange(TRequest request, out bool isAccount, out bool isGroup)
        {
            isAccount = false;
            isGroup = false;

            var name = request.GetType().Name;
            var isMutation = name.StartsWith("Create", StringComparison.Ordinal)
                             || name.StartsWith("Update", StringComparison.Ordinal)
                             || name.StartsWith("Delete", StringComparison.Ordinal)
                             || name.StartsWith("Move", StringComparison.Ordinal);
            if (!isMutation)
                return false;

            if (name.Contains("GlAccountMaster", StringComparison.Ordinal)) { isAccount = true; return true; }
            if (name.Contains("AccountGroup", StringComparison.Ordinal)) { isGroup = true; return true; }
            return false;
        }

        private static int? ExtractId(TRequest request)
        {
            var prop = request.GetType().GetProperty("Id");
            if (prop != null && prop.PropertyType == typeof(int))
            {
                var value = (int)prop.GetValue(request)!;
                return value > 0 ? value : null;
            }
            return null;
        }
    }
}
