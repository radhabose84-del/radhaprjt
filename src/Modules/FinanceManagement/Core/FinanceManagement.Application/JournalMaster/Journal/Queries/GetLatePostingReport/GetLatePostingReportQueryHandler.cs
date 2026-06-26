using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournal;
using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.Journal.Queries.GetLatePostingReport
{
    public class GetLatePostingReportQueryHandler
        : IRequestHandler<GetLatePostingReportQuery, ApiResponseDTO<List<LatePostingReportDto>>>
    {
        private readonly IJournalQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;

        public GetLatePostingReportQueryHandler(
            IJournalQueryRepository queryRepository,
            IIPAddressService ipAddressService,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<LatePostingReportDto>>> Handle(
            GetLatePostingReportQuery request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");

            var (data, totalCount) = await _queryRepository.GetLatePostingReportAsync(
                request.PageNumber,
                request.PageSize,
                companyId,
                request.AccountingPeriodId,
                request.FromDate,
                request.ToDate,
                request.SortBy,
                request.SortDirection);

            await _mediator.Publish(new AuditLogsDomainEvent(
                actionDetail: "GetLatePostingReportQuery",
                actionCode:   "Get",
                actionName:   data.Count.ToString(),
                details:      $"Late-posting report fetched ({data.Count} of {totalCount}) for Company {companyId}.",
                module:       "JournalHeader"
            ), cancellationToken);

            return new ApiResponseDTO<List<LatePostingReportDto>>
            {
                IsSuccess  = true,
                Message    = "Late-posting report retrieved successfully.",
                Data       = data,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize   = request.PageSize
            };
        }
    }
}
