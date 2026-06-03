using MediatR;
using PurchaseManagement.Application.BarcodeAllocation.Dto;
using PurchaseManagement.Application.Common.Interfaces.IBarcodeAllocation;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.BarcodeAllocation.Queries.GetBarcodeAllocationById
{
    public class GetBarcodeAllocationByIdQueryHandler : IRequestHandler<GetBarcodeAllocationByIdQuery, BarcodeAllocationDto?>
    {
        private readonly IBarcodeAllocationQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetBarcodeAllocationByIdQueryHandler(IBarcodeAllocationQueryRepository queryRepository, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<BarcodeAllocationDto?> Handle(GetBarcodeAllocationByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetBarcodeAllocationByIdQuery",
                actionName: result.Id.ToString(),
                details: $"Barcode allocation details {result.Id} was fetched.",
                module: "BarcodeAllocation"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
