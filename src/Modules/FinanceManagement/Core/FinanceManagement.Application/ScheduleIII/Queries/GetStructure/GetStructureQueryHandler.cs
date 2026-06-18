using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Application.ScheduleIII.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Queries.GetStructure
{
    public class GetStructureQueryHandler : IRequestHandler<GetStructureQuery, ScheduleIIIHeaderDto?>
    {
        private readonly IScheduleIIIQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;

        public GetStructureQueryHandler(
            IScheduleIIIQueryRepository queryRepository,
            IIPAddressService ipAddressService,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
        }

        public async Task<ScheduleIIIHeaderDto?> Handle(GetStructureQuery request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");
            var divisionId = _ipAddressService.GetDivisionId()
                ?? throw new ExceptionRules("No active division in session.");

            var result = await _queryRepository.GetStructureAsync(companyId, divisionId);

            if (result == null)
                return null;

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetStructure",
                actionCode: "GetStructureQuery",
                actionName: result.Id.ToString(),
                details: $"Schedule III structure {result.Id} was fetched.",
                module: "ScheduleIIIMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
