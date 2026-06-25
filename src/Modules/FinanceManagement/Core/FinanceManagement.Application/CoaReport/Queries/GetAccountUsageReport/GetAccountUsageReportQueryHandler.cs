using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.ICoaReport;
using FinanceManagement.Application.CoaReport.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.CoaReport.Queries.GetAccountUsageReport
{
    public class GetAccountUsageReportQueryHandler : IRequestHandler<GetAccountUsageReportQuery, ApiResponseDTO<List<AccountUsageItemDto>>>
    {
        private readonly ICoaReportQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;

        public GetAccountUsageReportQueryHandler(
            ICoaReportQueryRepository queryRepository,
            IIPAddressService ipAddressService,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<AccountUsageItemDto>>> Handle(GetAccountUsageReportQuery request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");

            var months = request.MonthsSincePosting > 0 ? request.MonthsSincePosting : 12;
            var rows = await _queryRepository.GetAccountUsageAsync(companyId, months, cancellationToken);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAccountUsageReportQuery",
                actionCode: "Get",
                actionName: rows.Count.ToString(),
                details: $"COA account-usage report ({months}-month window) was fetched.",
                module: "CoaReport"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<AccountUsageItemDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = rows,
                TotalCount = rows.Count
            };
        }
    }
}
