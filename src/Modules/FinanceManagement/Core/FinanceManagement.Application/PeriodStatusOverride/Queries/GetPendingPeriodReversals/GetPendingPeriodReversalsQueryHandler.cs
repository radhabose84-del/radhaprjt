using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IPeriodStatusOverride;
using FinanceManagement.Application.PeriodStatusOverride.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.PeriodStatusOverride.Queries.GetPendingPeriodReversals
{
    public class GetPendingPeriodReversalsQueryHandler : IRequestHandler<GetPendingPeriodReversalsQuery, IReadOnlyList<PeriodStatusOverrideDto>>
    {
        private readonly IPeriodStatusOverrideQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;

        public GetPendingPeriodReversalsQueryHandler(
            IPeriodStatusOverrideQueryRepository queryRepository,
            IIPAddressService ipAddressService,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<PeriodStatusOverrideDto>> Handle(GetPendingPeriodReversalsQuery request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");

            var result = await _queryRepository.GetPendingForCompanyAsync(companyId, cancellationToken);

            await _mediator.Publish(new AuditLogsDomainEvent(
                "GetAll", "GetPendingPeriodReversalsQuery", result.Count.ToString(),
                $"Pending PeriodStatusOverride list fetched ({result.Count} rows) for Company {companyId}.",
                "PeriodStatusOverride"), cancellationToken);

            return result;
        }
    }
}
