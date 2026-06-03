using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.BarcodeSeries.Dto;
using PurchaseManagement.Application.Common.Interfaces.IBarcodeSeries;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.BarcodeSeries.Queries.GetBarcodeSeries
{
    public class GetBarcodeSeriesQueryHandler : IRequestHandler<GetBarcodeSeriesQuery, ApiResponseDTO<List<BarcodeSeriesDto>>>
    {
        private readonly IBarcodeSeriesQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetBarcodeSeriesQueryHandler(IBarcodeSeriesQueryRepository queryRepository, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<BarcodeSeriesDto>>> Handle(GetBarcodeSeriesQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "Barcode series details were fetched.",
                module: "BarcodeSeries"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<BarcodeSeriesDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = data,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
