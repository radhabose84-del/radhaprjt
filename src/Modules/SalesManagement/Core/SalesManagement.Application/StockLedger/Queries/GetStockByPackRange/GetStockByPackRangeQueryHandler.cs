using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IStockLedger;
using SalesManagement.Application.StockLedger.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.StockLedger.Queries.GetStockByPackRange
{
    public class GetStockByPackRangeQueryHandler : IRequestHandler<GetStockByPackRangeQuery, ApiResponseDTO<object>>
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

        public async Task<ApiResponseDTO<object>> Handle(
            GetStockByPackRangeQuery request,
            CancellationToken cancellationToken)
        {
            object data;
            int count;
            string details;

            // Detail mode: all 4 params provided (ItemId, PackTypeId, StartPackNo, EndPackNo)
            if (request.ItemId.HasValue
                && request.PackTypeId.HasValue
                && request.StartPackNo.HasValue
                && request.EndPackNo.HasValue)
            {
                var result = await _reportRepository.GetByPackRangeAsync(
                    request.ItemId.Value,
                    request.PackTypeId.Value,
                    request.StartPackNo.Value,
                    request.EndPackNo.Value,
                    request.ProductionYear,
                    cancellationToken);
                data = result;
                count = result.Count;
                details = $"Stock Ledger detail for pack range {request.StartPackNo}-{request.EndPackNo} was fetched.";
            }
            else
            {
                // Summary mode: group by ItemId + PackTypeId with optional filters
                var result = await _reportRepository.GetPackRangeSummaryAsync(
                    request.ProductionYear,
                    request.ItemId,
                    request.PackTypeId,
                    request.StartPackNo,
                    request.EndPackNo,
                    cancellationToken);
                data = result;
                count = result.Count;
                details = $"Stock Ledger pack range summary was fetched. {count} group(s) returned.";
            }

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetStockByPackRange",
                actionCode: "Get",
                actionName: count.ToString(),
                details: details,
                module: "StockLedger"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<object>
            {
                IsSuccess = true,
                Message = "Stock Ledger pack range retrieved successfully.",
                Data = data,
                TotalCount = count
            };
        }
    }
}
