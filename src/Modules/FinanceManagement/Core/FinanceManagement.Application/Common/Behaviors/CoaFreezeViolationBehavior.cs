using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.ICoaFreeze;
using MediatR;

namespace FinanceManagement.Application.Common.Behaviors
{
    // Turns the DB trigger's RAISERROR('COA_FREEZE_VIOLATION') into a friendly 400 and records the
    // blocked attempt (the "Blocked Attempts" card). The DB trigger remains the real enforcement (AC4);
    // this only improves the API response for UI/API callers.
    public class CoaFreezeViolationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly ICoaFreezeViolationLog _violationLog;
        private readonly IIPAddressService _ipAddressService;

        public CoaFreezeViolationBehavior(ICoaFreezeViolationLog violationLog, IIPAddressService ipAddressService)
        {
            _violationLog = violationLog;
            _ipAddressService = ipAddressService;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            try
            {
                return await next();
            }
            catch (Exception ex) when (IsFreezeViolation(ex))
            {
                var companyId = _ipAddressService.GetCompanyId() ?? 0;
                var userId = _ipAddressService.GetUserId();

                try { await _violationLog.LogAsync(companyId, userId, request.GetType().Name, cancellationToken); }
                catch { /* logging is best-effort — never mask the freeze message */ }

                throw new ExceptionRules("Chart of Accounts is frozen — structural changes are blocked.");
            }
        }

        private static bool IsFreezeViolation(Exception ex)
        {
            for (Exception? e = ex; e is not null; e = e.InnerException)
                if (e.Message.Contains("COA_FREEZE_VIOLATION", StringComparison.OrdinalIgnoreCase))
                    return true;
            return false;
        }
    }
}
