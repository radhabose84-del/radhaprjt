using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using FinanceManagement.Application.CoaFreeze.Dto;
using FinanceManagement.Application.Common.Interfaces.ICoaFreeze;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.CoaFreeze.Queries.GetCoaFreezeState
{
    public class GetCoaFreezeStateQueryHandler : IRequestHandler<GetCoaFreezeStateQuery, CoaFreezeStateDto>
    {
        private readonly ICoaFreezeQueryRepository _queryRepository;
        private readonly ICoaFreezeViolationLog _violationLog;
        private readonly IUserLookup _userLookup;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;

        public GetCoaFreezeStateQueryHandler(
            ICoaFreezeQueryRepository queryRepository,
            ICoaFreezeViolationLog violationLog,
            IUserLookup userLookup,
            IIPAddressService ipAddressService,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _violationLog = violationLog;
            _userLookup = userLookup;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
        }

        public async Task<CoaFreezeStateDto> Handle(GetCoaFreezeStateQuery request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId() ?? 0;

            var state = await _queryRepository.GetStateAsync(companyId, cancellationToken);
            var triggersActive = await _queryRepository.AreTriggersActiveAsync(cancellationToken);
            var (totalAccounts, totalGroups) = await _queryRepository.GetCoaCountsAsync(companyId, cancellationToken);

            var dto = new CoaFreezeStateDto
            {
                IsFrozen = state?.IsFrozen ?? false,
                FrozenByUserId = state?.FrozenByUserId,
                FrozenOn = state?.FrozenOn,
                UnfreezeWindowExpiry = state?.UnfreezeWindowExpiry,
                AutoReFreezeAt = state?.UnfreezeWindowExpiry,
                DbTriggerActive = triggersActive,
                TotalAccounts = totalAccounts,
                TotalAccountGroups = totalGroups
            };

            if (state?.IsFrozen == true && state.FrozenOn.HasValue)
                dto.BlockedAttemptsSinceFreeze =
                    await _violationLog.CountSinceAsync(companyId, state.FrozenOn.Value, cancellationToken);

            if (state?.FrozenByUserId is int frozenBy && frozenBy > 0)
            {
                var user = await _userLookup.GetByIdAsync(frozenBy, cancellationToken);
                if (user != null)
                    dto.FrozenByName = $"{user.FirstName} {user.LastName}".Trim();
            }

            await _mediator.Publish(new AuditLogsDomainEvent(
                actionDetail: "GetCoaFreezeStateQuery",
                actionCode: "Get",
                actionName: dto.IsFrozen ? "FROZEN" : "OPEN",
                details: $"COA freeze state fetched (frozen={dto.IsFrozen}, triggers={dto.DbTriggerActive}).",
                module: "CoaFreeze"), cancellationToken);

            return dto;
        }
    }
}
