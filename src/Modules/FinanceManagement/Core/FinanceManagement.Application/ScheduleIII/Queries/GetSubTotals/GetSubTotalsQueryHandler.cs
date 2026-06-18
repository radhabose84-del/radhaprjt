using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Application.ScheduleIII.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Queries.GetSubTotals
{
    public class GetSubTotalsQueryHandler : IRequestHandler<GetSubTotalsQuery, ApiResponseDTO<List<ScheduleIIISubTotalDto>>>
    {
        private readonly IScheduleIIIQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IIPAddressService _ipAddressService;

        public GetSubTotalsQueryHandler(
            IScheduleIIIQueryRepository queryRepository,
            IMediator mediator,
            IIPAddressService ipAddressService)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
            _ipAddressService = ipAddressService;
        }

        public async Task<ApiResponseDTO<List<ScheduleIIISubTotalDto>>> Handle(GetSubTotalsQuery request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");
            var divisionId = _ipAddressService.GetDivisionId()
                ?? throw new ExceptionRules("No active division in session.");

            var data = await _queryRepository.GetSubTotalsAsync(companyId, divisionId);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetSubTotals",
                actionCode: "GetSubTotalsQuery",
                actionName: data.Count.ToString(),
                details: "Schedule III sub-totals were fetched.",
                module: "ScheduleIIISubTotal"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<ScheduleIIISubTotalDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = data
            };
        }
    }
}
