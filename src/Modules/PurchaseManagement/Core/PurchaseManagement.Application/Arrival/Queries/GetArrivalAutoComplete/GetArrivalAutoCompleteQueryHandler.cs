using MediatR;
using PurchaseManagement.Application.Arrival.Dto;
using PurchaseManagement.Application.Common.Interfaces.IArrival;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.Arrival.Queries.GetArrivalAutoComplete
{
    public class GetArrivalAutoCompleteQueryHandler : IRequestHandler<GetArrivalAutoCompleteQuery, IReadOnlyList<ArrivalLookupDto>>
    {
        private readonly IArrivalQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetArrivalAutoCompleteQueryHandler(IArrivalQueryRepository queryRepository, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<ArrivalLookupDto>> Handle(GetArrivalAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term ?? string.Empty, cancellationToken);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetArrivalAutoCompleteQuery",
                actionName: result.Count.ToString(),
                details: "Arrival details was fetched.",
                module: "Arrival");
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
