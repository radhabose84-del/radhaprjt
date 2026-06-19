using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Application.ScheduleIII.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Queries.Get03BDropdownPreview
{
    public class Get03BDropdownPreviewQueryHandler : IRequestHandler<Get03BDropdownPreviewQuery, Preview03BDto>
    {
        private readonly IScheduleIIIQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IIPAddressService _ipAddressService;

        public Get03BDropdownPreviewQueryHandler(
            IScheduleIIIQueryRepository queryRepository,
            IMediator mediator,
            IIPAddressService ipAddressService)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
            _ipAddressService = ipAddressService;
        }

        public async Task<Preview03BDto> Handle(Get03BDropdownPreviewQuery request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");
            var divisionId = _ipAddressService.GetDivisionId()
                ?? throw new ExceptionRules("No active division in session.");

            var result = await _queryRepository.Get03BPreviewAsync(companyId, divisionId);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Get03BDropdownPreview",
                actionCode: "Get03BDropdownPreviewQuery",
                actionName: (result.BalanceSheetLeaves.Count + result.ProfitAndLossLeaves.Count).ToString(),
                details: "Schedule III 03B dropdown preview was fetched.",
                module: "ScheduleIIIHeader"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
