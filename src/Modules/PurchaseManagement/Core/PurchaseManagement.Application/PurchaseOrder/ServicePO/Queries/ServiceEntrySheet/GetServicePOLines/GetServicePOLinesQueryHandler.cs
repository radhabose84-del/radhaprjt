using AutoMapper;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ServicePO;
using PurchaseManagement.Domain.Events;
using MediatR;

namespace PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.GetServicePOLines
{
    public class GetServicePOLinesQueryHandler : IRequestHandler<GetServicePOLinesQuery, IReadOnlyList<GetServicePOLinesDto>>
    {

        private readonly IServicePurchaseOrderQueryRepository _servicePurchaseOrderQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetServicePOLinesQueryHandler(IServicePurchaseOrderQueryRepository servicePurchaseOrderQueryRepository, IMapper mapper, IMediator mediator)
        {
            _servicePurchaseOrderQueryRepository = servicePurchaseOrderQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<GetServicePOLinesDto>> Handle(
            GetServicePOLinesQuery request,
            CancellationToken ct)
        {
            var rows = await _servicePurchaseOrderQueryRepository.GetLinesByPoIdAsync(
                request.POId, ct);

            var data = rows?.ToList() ?? new List<GetServicePOLinesDto>();

            await _mediator.Publish(new AuditLogsDomainEvent(
                "GetServicePOLinesByPoId",
                "GetServicePOLinesQuery",
                $"Fetched {data.Count} service PO line(s)",
                $"Lines for PurchaseOrderId={request.POId}.",
                "SERVICE PO"), ct);

            return data;
        }
    }
}
