using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Application.ScheduleIII.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Queries.GetLinesAutoComplete
{
    public class GetLinesAutoCompleteQueryHandler
        : IRequestHandler<GetLinesAutoCompleteQuery, ApiResponseDTO<List<ScheduleIIILineLookupDto>>>
    {
        private readonly IScheduleIIIQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;

        public GetLinesAutoCompleteQueryHandler(
            IScheduleIIIQueryRepository queryRepository,
            IIPAddressService ipAddressService,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<ScheduleIIILineLookupDto>>> Handle(GetLinesAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");
            var divisionId = _ipAddressService.GetDivisionId()
                ?? throw new ExceptionRules("No active division in session.");

            var data = await _queryRepository.GetLinesAutoCompleteAsync(companyId, divisionId, request.Term);

            await _mediator.Publish(new AuditLogsDomainEvent(
                actionDetail: "GetLinesAutoComplete", actionCode: "GetLinesAutoCompleteQuery", actionName: data.Count.ToString(),
                details: "Schedule III structure lines (autocomplete) were fetched.", module: "ScheduleIIIDetail"), cancellationToken);

            return new ApiResponseDTO<List<ScheduleIIILineLookupDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = data,
                TotalCount = data.Count
            };
        }
    }
}
