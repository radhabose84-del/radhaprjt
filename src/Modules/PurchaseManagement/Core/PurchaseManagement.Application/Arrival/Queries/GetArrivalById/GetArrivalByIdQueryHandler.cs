using MediatR;
using PurchaseManagement.Application.Arrival.Dto;
using PurchaseManagement.Application.Common.Interfaces.IArrival;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.Arrival.Queries.GetArrivalById
{
    public class GetArrivalByIdQueryHandler : IRequestHandler<GetArrivalByIdQuery, ArrivalDto?>
    {
        private readonly IArrivalQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetArrivalByIdQueryHandler(IArrivalQueryRepository queryRepository, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<ArrivalDto?> Handle(GetArrivalByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetArrivalByIdQuery",
                actionName: result.Id.ToString(),
                details: $"Arrival details {result.Id} was fetched.",
                module: "Arrival");
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
