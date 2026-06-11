using MediatR;
using PurchaseManagement.Application.Arrival.Dto;
using PurchaseManagement.Application.Common.Interfaces.IArrival;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.Arrival.Queries.GetArrivalLastLotNo
{
    public class GetArrivalLastLotNoQueryHandler : IRequestHandler<GetArrivalLastLotNoQuery, ArrivalLastLotNoDto?>
    {
        private readonly IArrivalQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetArrivalLastLotNoQueryHandler(IArrivalQueryRepository queryRepository, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<ArrivalLastLotNoDto?> Handle(GetArrivalLastLotNoQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetLastLotNoAsync();

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetLastLotNo",
                actionCode: "GetArrivalLastLotNoQuery",
                actionName: (result?.LotNo ?? 0).ToString(),
                details: "Arrival last lot number was fetched.",
                module: "Arrival");
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
