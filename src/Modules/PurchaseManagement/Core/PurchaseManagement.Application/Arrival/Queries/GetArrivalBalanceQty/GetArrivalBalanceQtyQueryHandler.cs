using MediatR;
using PurchaseManagement.Application.Arrival.Dto;
using PurchaseManagement.Application.Common.Interfaces.IArrival;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.Arrival.Queries.GetArrivalBalanceQty
{
    public class GetArrivalBalanceQtyQueryHandler : IRequestHandler<GetArrivalBalanceQtyQuery, IReadOnlyList<ArrivalBalanceQtyDto>>
    {
        private readonly IArrivalQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetArrivalBalanceQtyQueryHandler(IArrivalQueryRepository queryRepository, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<ArrivalBalanceQtyDto>> Handle(GetArrivalBalanceQtyQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetBalanceQuantitiesAsync(request.RawMaterialPOId);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetBalanceQty",
                actionCode: "GetArrivalBalanceQtyQuery",
                actionName: result.Count.ToString(),
                details: $"Arrival balance quantity for Raw Material PO {request.RawMaterialPOId} was fetched.",
                module: "Arrival");
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
