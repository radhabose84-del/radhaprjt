using Contracts.Common;
using MediatR;
using Contracts.Interfaces;
using SalesManagement.Application.Common.Interfaces.IStockLedger;
using SalesManagement.Application.StockLedger.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.StockLedger.Queries.GetStockLedgerReport
{
    public class GetStockLedgerReportQueryHandler : IRequestHandler<GetStockLedgerReportQuery, ApiResponseDTO<List<StockLedgerReportDto>>>
    {
        private readonly IStockLedgerReportRepository _reportRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;

        public GetStockLedgerReportQueryHandler(
            IStockLedgerReportRepository reportRepository,
            IIPAddressService ipAddressService,
            IMediator mediator)
        {
            _reportRepository = reportRepository;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<StockLedgerReportDto>>> Handle(
            GetStockLedgerReportQuery request,
            CancellationToken cancellationToken)
        {
            var unitId = _ipAddressService.GetUnitId();

            var (data, totalCount) = await _reportRepository.GetReportAsync(
                unitId ?? 0,
                request.PageNumber,
                request.PageSize,
                request.ItemId,
                request.LotId,
                request.WarehouseId,
                request.BinId,
                request.StatusId,
                request.PackNo,
                request.DateFrom,
                request.DateTo);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetStockLedgerReport",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "Stock Ledger report was fetched.",
                module: "StockLedger"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<StockLedgerReportDto>>
            {
                IsSuccess = true,
                Message = "Stock Ledger report retrieved successfully.",
                Data = data,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
