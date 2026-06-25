using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IPeriodStatusOverride;
using FinanceManagement.Application.PeriodStatusOverride.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.PeriodStatusOverride.Queries.GetPeriodStatusHistory
{
    public class GetPeriodStatusHistoryQueryHandler : IRequestHandler<GetPeriodStatusHistoryQuery, IReadOnlyList<PeriodStatusOverrideDto>>
    {
        private readonly IPeriodStatusOverrideQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;

        public GetPeriodStatusHistoryQueryHandler(
            IPeriodStatusOverrideQueryRepository queryRepository,
            IIPAddressService ipAddressService,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<PeriodStatusOverrideDto>> Handle(GetPeriodStatusHistoryQuery request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");

            var result = await _queryRepository.GetHistoryForPeriodAsync(request.PeriodId, companyId, cancellationToken);

            await _mediator.Publish(new AuditLogsDomainEvent(
                "GetAll", "GetPeriodStatusHistoryQuery", result.Count.ToString(),
                $"PeriodStatusOverride history for Period {request.PeriodId} fetched ({result.Count} rows).",
                "PeriodStatusOverride"), cancellationToken);

            return result;
        }
    }
}
