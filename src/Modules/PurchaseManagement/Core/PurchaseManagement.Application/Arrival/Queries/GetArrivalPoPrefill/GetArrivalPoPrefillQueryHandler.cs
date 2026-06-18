using MediatR;
using PurchaseManagement.Application.Arrival.Dto;
using PurchaseManagement.Application.Common.Interfaces.IArrival;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.Arrival.Queries.GetArrivalPoPrefill
{
    public class GetArrivalPoPrefillQueryHandler : IRequestHandler<GetArrivalPoPrefillQuery, ArrivalPoPrefillDto>
    {
        private readonly IArrivalQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetArrivalPoPrefillQueryHandler(IArrivalQueryRepository queryRepository, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<ArrivalPoPrefillDto> Handle(GetArrivalPoPrefillQuery request, CancellationToken cancellationToken)
        {
            var items = await _queryRepository.GetBalanceQuantitiesAsync(request.RawMaterialPOId);
            var approvedFreight = await _queryRepository.GetApprovedFreightByPoAsync(request.RawMaterialPOId);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetPoPrefill",
                actionCode: "GetArrivalPoPrefillQuery",
                actionName: items.Count.ToString(),
                details: $"Arrival PO prefill (balance qty + approved freight) for Raw Material PO {request.RawMaterialPOId} was fetched.",
                module: "Arrival");
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ArrivalPoPrefillDto { Items = items, ApprovedFreight = approvedFreight };
        }
    }
}
