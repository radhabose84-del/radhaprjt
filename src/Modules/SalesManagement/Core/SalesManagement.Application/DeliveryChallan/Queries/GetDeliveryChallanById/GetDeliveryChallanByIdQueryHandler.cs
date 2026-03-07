using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDeliveryChallan;
using SalesManagement.Application.DeliveryChallan.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.DeliveryChallan.Queries.GetDeliveryChallanById
{
    public class GetDeliveryChallanByIdQueryHandler : IRequestHandler<GetDeliveryChallanByIdQuery, DeliveryChallanHeaderDto?>
    {
        private readonly IDeliveryChallanQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetDeliveryChallanByIdQueryHandler(
            IDeliveryChallanQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<DeliveryChallanHeaderDto?> Handle(GetDeliveryChallanByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetDeliveryChallanByIdQuery",
                actionName: result.Id.ToString(),
                details: $"Delivery Challan details {result.Id} was fetched.",
                module: "DeliveryChallan");
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
