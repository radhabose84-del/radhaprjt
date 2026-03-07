using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDeliveryChallan;
using SalesManagement.Application.DeliveryChallan.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.DeliveryChallan.Queries.GetStoOpenQty
{
    public class GetStoOpenQtyQueryHandler : IRequestHandler<GetStoOpenQtyQuery, StoOpenQtyDto?>
    {
        private readonly IDeliveryChallanQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetStoOpenQtyQueryHandler(
            IDeliveryChallanQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<StoOpenQtyDto?> Handle(GetStoOpenQtyQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetStoOpenQtyAsync(request.StoDetailId);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetStoOpenQty",
                actionCode: "GetStoOpenQtyQuery",
                actionName: request.StoDetailId.ToString(),
                details: $"STO open quantity for detail {request.StoDetailId} was fetched.",
                module: "DeliveryChallan");
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
