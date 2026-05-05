using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ServicePO;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.GetScheduleByPoIdandSeviceidandServiceItemid
{
    public class GetSESCreateSourceQueryHandler : IRequestHandler<GetSESCreateSourceQuery, SesFromScheduleRawDto?>
    {
        private readonly IServicePurchaseOrderQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetSESCreateSourceQueryHandler(
            IServicePurchaseOrderQueryRepository queryRepository,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<SesFromScheduleRawDto?> Handle(GetSESCreateSourceQuery request, CancellationToken cancellationToken)
        {
            var dto = await _queryRepository.GetSesCreateSourceAsync(
                request.PurchaseOrderId,
                request.ScheduleNo,
                request.ServiceItemId,
                cancellationToken);

            await _mediator.Publish(new AuditLogsDomainEvent(
                actionDetail: "GetSesCreateSource",
                actionCode: "GetSESCreateSourceQuery",
                actionName: dto?.ScheduleId.ToString() ?? "0",
                details: $"SES create-source fetched for PO={request.PurchaseOrderId}, ScheduleNo={request.ScheduleNo}, ServiceItemId={request.ServiceItemId}.",
                module: "ServiceEntrySheet"), cancellationToken);

            return dto;
        }
    }
}
