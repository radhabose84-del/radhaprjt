using MediatR;
using SalesManagement.Application.Common.Interfaces.IDeliveryChallan;
using SalesManagement.Application.DeliveryChallan.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.DeliveryChallan.Queries.GetDeliveryChallanForReceipt
{
    public class GetDeliveryChallanForReceiptQueryHandler : IRequestHandler<GetDeliveryChallanForReceiptQuery, IReadOnlyList<DeliveryChallanLookupDto>>
    {
        private readonly IDeliveryChallanQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetDeliveryChallanForReceiptQueryHandler(
            IDeliveryChallanQueryRepository queryRepository,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<DeliveryChallanLookupDto>> Handle(GetDeliveryChallanForReceiptQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetForReceiptAsync(request.Term ?? string.Empty, cancellationToken);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetDeliveryChallanForReceiptQuery",
                actionName: result.Count.ToString(),
                details: "DeliveryChallan for receipt was fetched.",
                module: "DeliveryChallan");
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
