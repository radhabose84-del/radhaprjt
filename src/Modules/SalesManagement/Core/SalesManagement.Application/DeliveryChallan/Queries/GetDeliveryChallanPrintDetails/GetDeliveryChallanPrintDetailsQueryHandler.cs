using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDeliveryChallan;
using SalesManagement.Application.DeliveryChallan.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.DeliveryChallan.Queries.GetDeliveryChallanPrintDetails
{
    public class GetDeliveryChallanPrintDetailsQueryHandler : IRequestHandler<GetDeliveryChallanPrintDetailsQuery, DeliveryChallanPrintDto?>
    {
        private readonly IDeliveryChallanQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetDeliveryChallanPrintDetailsQueryHandler(
            IDeliveryChallanQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<DeliveryChallanPrintDto?> Handle(GetDeliveryChallanPrintDetailsQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetPrintDetailsAsync(request.Id);

            if (result == null)
                return null;

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetPrintDetails",
                actionCode: "DC_PRINT",
                actionName: request.Id.ToString(),
                details: $"Delivery Challan print details {request.Id} was fetched.",
                module: "DeliveryChallan");
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
