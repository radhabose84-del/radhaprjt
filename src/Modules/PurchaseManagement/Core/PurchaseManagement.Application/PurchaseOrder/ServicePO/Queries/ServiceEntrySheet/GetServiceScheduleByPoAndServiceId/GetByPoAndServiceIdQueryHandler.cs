using AutoMapper;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ServicePO;
using PurchaseManagement.Domain.Events;
using MediatR;

namespace PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.GetServiceScheduleByPoAndServiceId
{
    public class GetSchedulesByPoAndServiceIdHandler : IRequestHandler<GetByPoAndServiceIdQuery, List<ServiceScheduleDto>>
    {


        private readonly IServicePurchaseOrderQueryRepository _servicePurchaseOrderQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetSchedulesByPoAndServiceIdHandler(IServicePurchaseOrderQueryRepository servicePurchaseOrderQueryRepository, IMapper mapper, IMediator mediator)
        {
            _servicePurchaseOrderQueryRepository = servicePurchaseOrderQueryRepository;
            _mapper = mapper;
            _mediator = mediator;

        }
      

        public async Task<List<ServiceScheduleDto>> Handle(GetByPoAndServiceIdQuery request, CancellationToken ct)
        {
            var rows = await _servicePurchaseOrderQueryRepository.GetByPoAndServiceIdAsync(request.PoId, request.ServiceId, ct); // IEnumerable<ServiceScheduleDto>
            var data = rows?.ToList() ?? new List<ServiceScheduleDto>();

            await _mediator.Publish(new AuditLogsDomainEvent(
                "GetByPoAndServiceId",
                "GetSchedulesByPoAndServiceIdQuery",
                $"Fetched {data.Count} schedule(s)",
                $"Schedules for PO={request.PoId}, ServiceId={request.ServiceId}.",
                "SES PO"), ct);

            return data;
        }
    }
}