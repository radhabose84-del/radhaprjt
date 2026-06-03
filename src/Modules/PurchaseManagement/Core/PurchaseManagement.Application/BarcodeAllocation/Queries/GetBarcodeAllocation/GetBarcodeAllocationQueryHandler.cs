using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.BarcodeAllocation.Dto;
using PurchaseManagement.Application.Common.Interfaces.IBarcodeAllocation;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.BarcodeAllocation.Queries.GetBarcodeAllocation
{
    public class GetBarcodeAllocationQueryHandler : IRequestHandler<GetBarcodeAllocationQuery, ApiResponseDTO<List<BarcodeAllocationDto>>>
    {
        private readonly IBarcodeAllocationQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetBarcodeAllocationQueryHandler(IBarcodeAllocationQueryRepository queryRepository, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<BarcodeAllocationDto>>> Handle(GetBarcodeAllocationQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetAllAsync(request.PageNumber, request.PageSize, request.SearchTerm);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "Barcode allocation details were fetched.",
                module: "BarcodeAllocation"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<BarcodeAllocationDto>>
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
