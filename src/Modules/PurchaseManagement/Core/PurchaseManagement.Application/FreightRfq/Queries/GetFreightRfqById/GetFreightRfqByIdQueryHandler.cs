using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IFreightRfq;
using PurchaseManagement.Application.FreightRfq.Dto;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.FreightRfq.Queries.GetFreightRfqById
{
    public class GetFreightRfqByIdQueryHandler : IRequestHandler<GetFreightRfqByIdQuery, FreightRfqDto?>
    {
        private readonly IFreightRfqQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetFreightRfqByIdQueryHandler(IFreightRfqQueryRepository queryRepository, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<FreightRfqDto?> Handle(GetFreightRfqByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetFreightRfqByIdQuery",
                actionName: result.Id.ToString(),
                details: $"Freight RFQ details {result.Id} was fetched.",
                module: "FreightRfq"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
