using Contracts.Common;
using Contracts.Interfaces;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesOrder;
using SalesManagement.Application.SalesOrder.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesOrder.Queries.GetDiscountReport
{
    public class GetDiscountReportQueryHandler
        : IRequestHandler<GetDiscountReportQuery, ApiResponseDTO<SalesOrderDiscountReportDto>>
    {
        private readonly ISalesOrderQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;

        public GetDiscountReportQueryHandler(
            ISalesOrderQueryRepository queryRepository,
            IIPAddressService ipAddressService,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<SalesOrderDiscountReportDto>> Handle(
            GetDiscountReportQuery request, CancellationToken cancellationToken)
        {
            var statusFilter = string.IsNullOrWhiteSpace(request.StatusName) ? "Approved" : request.StatusName;
            var pageNumber   = request.PageNumber <= 0 ? 1  : request.PageNumber;
            var pageSize     = request.PageSize   <= 0 ? 50 : request.PageSize;

            // OrderUnitId is taken from the JWT (current user's unit) — not from the query.
            var orderUnitId = _ipAddressService.GetUnitId();

            var (report, totalCount) = await _queryRepository.GetDiscountReportAsync(
                request.FromDate,
                request.ToDate,
                statusFilter,
                orderUnitId,
                request.PartyId,
                request.AgentId,
                request.SalesGroupId,
                request.DiscountSource,
                pageNumber,
                pageSize,
                cancellationToken);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetDiscountReport",
                actionCode:   "GetDiscountReportQuery",
                actionName:   $"Page={pageNumber},Size={pageSize}",
                details:      $"Sales Order discount report fetched. RowsOnPage={report.Rows.Count}, TotalCount={totalCount}.",
                module:       "SalesOrder");
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<SalesOrderDiscountReportDto>
            {
                IsSuccess  = true,
                Message    = "Success",
                Data       = report,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize   = pageSize
            };
        }
    }
}
