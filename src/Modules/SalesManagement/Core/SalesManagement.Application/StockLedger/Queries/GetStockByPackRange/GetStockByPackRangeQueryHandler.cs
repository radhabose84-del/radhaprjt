using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IStockLedger;
using SalesManagement.Application.StockLedger.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.StockLedger.Queries.GetStockByPackRange
{
    public class GetStockByPackRangeQueryHandler : IRequestHandler<GetStockByPackRangeQuery, ApiResponseDTO<List<StockLedgerReportDto>>>
    {
        private readonly IStockLedgerReportRepository _reportRepository;
        private readonly IMediator _mediator;

        public GetStockByPackRangeQueryHandler(
            IStockLedgerReportRepository reportRepository,
            IMediator mediator)
        {
            _reportRepository = reportRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<StockLedgerReportDto>>> Handle(
            GetStockByPackRangeQuery request,
            CancellationToken cancellationToken)
        {
            var data = await _reportRepository.GetByPackRangeAsync(
                request.ItemId,
                request.PackTypeId,
                request.StartPackNo,
                request.EndPackNo,
                request.ProductionYear,
                cancellationToken);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetStockByPackRange",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: $"Stock Ledger by pack range {request.StartPackNo}-{request.EndPackNo} was fetched.",
                module: "StockLedger"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<StockLedgerReportDto>>
            {
                IsSuccess = true,
                Message = "Stock Ledger pack range retrieved successfully.",
                Data = data,
                TotalCount = data.Count
            };
        }
    }
}
