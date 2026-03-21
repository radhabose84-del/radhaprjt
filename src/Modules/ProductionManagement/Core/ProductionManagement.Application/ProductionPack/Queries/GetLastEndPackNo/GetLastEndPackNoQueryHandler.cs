using MediatR;
using ProductionManagement.Application.Common.Interfaces.IProductionPack;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.ProductionPack.Queries.GetLastEndPackNo
{
    public class GetLastEndPackNoQueryHandler : IRequestHandler<GetLastEndPackNoQuery, int>
    {
        private readonly IProductionQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetLastEndPackNoQueryHandler(
            IProductionQueryRepository queryRepository,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<int> Handle(GetLastEndPackNoQuery request, CancellationToken cancellationToken)
        {
            var lastEndPackNo = await _queryRepository.GetLastEndPackNoAsync(request.ProductionYear);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetLastEndPackNo",
                actionCode: "GetLastEndPackNoQuery",
                actionName: lastEndPackNo.ToString(),
                details: $"Last EndPackNo for ProductionYear {request.ProductionYear} was fetched.",
                module: "Production"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return lastEndPackNo;
        }
    }
}
