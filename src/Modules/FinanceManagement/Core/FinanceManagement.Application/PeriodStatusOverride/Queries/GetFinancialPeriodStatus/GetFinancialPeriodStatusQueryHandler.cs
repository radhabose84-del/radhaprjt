using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IPeriodStatusOverride;
using FinanceManagement.Application.PeriodStatusOverride.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.PeriodStatusOverride.Queries.GetFinancialPeriodStatus
{
    public class GetFinancialPeriodStatusQueryHandler : IRequestHandler<GetFinancialPeriodStatusQuery, FinancialPeriodStatusDto?>
    {
        private readonly IPeriodStatusOverrideQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;

        public GetFinancialPeriodStatusQueryHandler(
            IPeriodStatusOverrideQueryRepository queryRepository,
            IIPAddressService ipAddressService,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
        }

        public async Task<FinancialPeriodStatusDto?> Handle(GetFinancialPeriodStatusQuery request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");

            var dto = await _queryRepository.GetPeriodStatusAsync(request.PeriodId, companyId, cancellationToken);
            if (dto == null) return null;

            await _mediator.Publish(new AuditLogsDomainEvent(
                "GetById", "GetFinancialPeriodStatusQuery", dto.PeriodId.ToString(),
                $"AccountingPeriod status {dto.PeriodId} was fetched.",
                "AccountingPeriod"), cancellationToken);

            return dto;
        }
    }
}
